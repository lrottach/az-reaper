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
    private readonly string _lifetimeTagName = configuration.GetValue<string>("LifetimeTagName") ?? "CloudReaperLifetime";
    private readonly string _statusTagName = configuration.GetValue<string>("StatusTagName") ?? "CloudReaperStatus";
    private readonly string _deletionTimeTagName = configuration.GetValue<string>("DeletionTimeTagName") ?? "CloudReaperDeletionTime";

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

        if (!await PrepareResourceGroup())
        {
            State = null!;
            return;
        }

        await SetScheduleAsync(lifetimeMinutes.Value);
    }

    private async Task<int?> ValidateResourceGroupEligibility()
    {
        try
        {
            var rg = await azureResourceService.GetAzureResourceGroup(State.SubscriptionId!, State.ResourceGroupName!);

            if (rg is null)
            {
                logger.LogWarning("[EntityTrigger] Resource Group '{rg}' not found in subscription '{sub}'", State.ResourceGroupName, State.SubscriptionId);
                return null;
            }

            if (TagHandler.TryGetLifetimeMinutes(rg.Data.Tags, _lifetimeTagName, out var lifetimeMinutes))
            {
                logger.LogInformation("[EntityTrigger] Resource Group '{rg}' has valid lifetime tag: {minutes} minutes", rg.Data.Name, lifetimeMinutes);
                return lifetimeMinutes;
            }

            logger.LogWarning("[EntityTrigger] Resource Group '{rg}' does not have a valid '{tag}' tag", rg.Data.Name, _lifetimeTagName);
            return null;
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "[EntityTrigger] Failed to validate Resource Group '{rg}' in subscription '{sub}'", State.ResourceGroupName, State.SubscriptionId);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityTrigger] Unexpected error validating Resource Group '{rg}' in subscription '{sub}'", State.ResourceGroupName, State.SubscriptionId);
            throw;
        }
    }

    private async Task<bool> PrepareResourceGroup()
    {
        try
        {
            await azureResourceService.ApplyResourceGroupTags(State.SubscriptionId!, State.ResourceGroupName!,
                _statusTagName, "Confirmed");
            logger.LogInformation("[EntityTrigger] Applied '{tag}' = 'Confirmed' to Resource Group '{rg}'", _statusTagName, State.ResourceGroupName);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            logger.LogWarning("[EntityTrigger] Resource Group '{rg}' was deleted before approval tag could be applied. Removing entity.", State.ResourceGroupName);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityTrigger] Failed to apply approval tag to Resource Group '{rg}'", State.ResourceGroupName);
            throw;
        }
    }

    private async Task SetScheduleAsync(int lifetimeMinutes)
    {
        var signalTime = DateTimeOffset.UtcNow.AddMinutes(lifetimeMinutes);
        var signalOptions = new SignalEntityOptions
        {
            SignalTime = signalTime
        };
        Context.SignalEntity(Context.Id, nameof(DeleteResourceAsync), signalOptions);

        State.Scheduled = true;
        State.DeletionScheduledAt = signalTime;

        try
        {
            await azureResourceService.ApplyResourceGroupTags(State.SubscriptionId!, State.ResourceGroupName!,
                _deletionTimeTagName, signalTime.ToString("o"));
            logger.LogInformation("[EntityTrigger] Applied '{tag}' = '{time}' to Resource Group '{rg}'", _deletionTimeTagName, signalTime.ToString("o"), State.ResourceGroupName);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            logger.LogWarning("[EntityTrigger] Resource Group '{rg}' was deleted before deletion time tag could be applied", State.ResourceGroupName);
        }

        logger.LogInformation("[EntityTrigger] Scheduled deletion of Resource Group '{rg}' at {time}", State.ResourceGroupName, signalTime);
    }

    public async Task DeleteResourceAsync()
    {
        // Re-fetch the resource group to check current tag state
        var rg = await azureResourceService.GetAzureResourceGroup(State.SubscriptionId!, State.ResourceGroupName!);

        if (rg is null)
        {
            logger.LogWarning("[EntityTrigger] Resource Group '{rg}' no longer exists. Cleaning up entity.", State.ResourceGroupName);
            State = null!;
            return;
        }

        // Check if the lifetime tag was removed (user cancelled deletion)
        if (!rg.Data.Tags.ContainsKey(_lifetimeTagName))
        {
            logger.LogInformation("[EntityTrigger] Lifetime tag '{tag}' removed from Resource Group '{rg}'. Cancelling scheduled deletion.",
                _lifetimeTagName, State.ResourceGroupName);

            // Clean up reaper metadata tags (best-effort — entity cleanup must not be blocked by tag removal failures)
            try
            {
                if (rg.Data.Tags.ContainsKey(_statusTagName))
                {
                    await azureResourceService.RemoveResourceGroupTag(State.SubscriptionId!, State.ResourceGroupName!, _statusTagName);
                }

                if (rg.Data.Tags.ContainsKey(_deletionTimeTagName))
                {
                    await azureResourceService.RemoveResourceGroupTag(State.SubscriptionId!, State.ResourceGroupName!, _deletionTimeTagName);
                }
            }
            catch (RequestFailedException ex)
            {
                logger.LogWarning(ex, "[EntityTrigger] Failed to remove cleanup tags from Resource Group '{rg}'. Entity will still be cleaned up.", State.ResourceGroupName);
            }

            State = null!;
            return;
        }

        // Lifetime tag still present - proceed with deletion
        try
        {
            await azureResourceService.DeleteResourceGroupAsync(State.SubscriptionId!, State.ResourceGroupName!);
            logger.LogInformation("[EntityTrigger] Deletion started for Resource Group '{rg}'", State.ResourceGroupName);
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
