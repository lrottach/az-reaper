using Azure;
using AzureReaper.Interfaces;
using AzureReaper.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AzureReaper.Common;

namespace AzureReaper.Entities;

// ReSharper disable once ClassNeverInstantiated.Global
public class AzureResourceEntity(
    ILogger<AzureResourceEntity> logger,
    IAzureResourceService azureResourceService,
    IConfiguration configuration) : TaskEntity<AzureResourceState>
{
    private readonly string _lifetimeTagName = configuration.GetValue<string>("LifetimeTagName") ?? "AzureReaperLifetime";
    private readonly string _statusTagName = configuration.GetValue<string>("StatusTagName") ?? "AzureReaperStatus";

    public async Task InitializeEntityAsync(ResourcePayload resourcePayload)
    {
        logger.LogInformation("[EntityTrigger] Entity initialized for Resource Id '{resourceId}'", resourcePayload.ResourceId);

        // Check if current entity is already scheduled
        if (State.Scheduled)
        {
            logger.LogWarning("[EntityTrigger] Entity already scheduled for Resource Id '{resourceId}'. Skipping.", resourcePayload.ResourceId);
            return;
        }

        // Write entity state
        State.ResourceGroupName = resourcePayload.ResourceGroupName;
        State.ResourceId = resourcePayload.ResourceId;
        State.SubscriptionId = resourcePayload.SubscriptionId;

        // Validate current Azure Resource Group
        logger.LogInformation("[EntityTrigger] Validating Resource Group eligibility for '{rg}'", resourcePayload.ResourceGroupName);
        var lifetimeMinutes = await ValidateResourceGroupEligibility();

        if (lifetimeMinutes is null)
        {
            logger.LogInformation("[EntityTrigger] Resource Group '{rg}' is not eligible. Removing entity.", resourcePayload.ResourceGroupName);
            State = null!;
            return;
        }

        await PrepareResourceGroup();
        SetSchedule(lifetimeMinutes.Value);
    }

    private async Task<int?> ValidateResourceGroupEligibility()
    {
        try
        {
            var rg = await azureResourceService.GetAzureResourceGroup(State.SubscriptionId, State.ResourceGroupName);

            if (TagHandler.TryGetLifetimeMinutes(rg.Data.Tags, _lifetimeTagName, out var lifetimeMinutes))
            {
                logger.LogInformation("[EntityTrigger] Resource Group '{rg}' has valid lifetime tag: {minutes} minutes", rg.Data.Name, lifetimeMinutes);
                return lifetimeMinutes;
            }

            logger.LogWarning("[EntityTrigger] Resource Group '{rg}' does not have a valid '{tag}' tag", rg.Data.Name, _lifetimeTagName);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityTrigger] Failed to validate Resource Group '{rg}' in subscription '{sub}'", State.ResourceGroupName, State.SubscriptionId);
            return null;
        }
    }

    private async Task PrepareResourceGroup()
    {
        try
        {
            await azureResourceService.ApplyResourceGroupTags(State.SubscriptionId, State.ResourceGroupName,
                _statusTagName, "confirmed");
            logger.LogInformation("[EntityTrigger] Applied '{tag}' = 'confirmed' to Resource Group '{rg}'", _statusTagName, State.ResourceGroupName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityTrigger] Failed to apply approval tag to Resource Group '{rg}'", State.ResourceGroupName);
            throw;
        }
    }

    private void SetSchedule(int lifetimeMinutes)
    {
        var signalTime = DateTimeOffset.UtcNow.AddMinutes(lifetimeMinutes);
        var signalOptions = new SignalEntityOptions
        {
            SignalTime = signalTime
        };
        Context.SignalEntity(Context.Id, nameof(DeleteResourceAsync), signalOptions);

        State.Scheduled = true;
        State.DeletionScheduledAt = signalTime;

        logger.LogInformation("[EntityTrigger] Scheduled deletion of Resource Group '{rg}' at {time}", State.ResourceGroupName, signalTime);
    }

    public async Task DeleteResourceAsync()
    {
        try
        {
            await azureResourceService.DeleteResourceGroupAsync(State.SubscriptionId, State.ResourceGroupName);
            logger.LogInformation("[EntityTrigger] Successfully deleted Resource Group '{rg}'", State.ResourceGroupName);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            logger.LogWarning("[EntityTrigger] Resource Group '{rg}' was already deleted", State.ResourceGroupName);
        }

        // Clean up entity from durable storage
        State = null!;
    }

    [Function(nameof(AzureResourceEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
    {
        return dispatcher.DispatchAsync<AzureResourceEntity>();
    }
}
