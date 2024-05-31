using AzureReaper.Interfaces;
using AzureReaper.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using AzureReaper.Common;

namespace AzureReaper.Entities;

// ReSharper disable once ClassNeverInstantiated.Global
public class AzureResourceEntity(ILogger<AzureResourceEntity> logger, IAzureResourceService azureResourceService)
    : TaskEntity<AzureResourceState>
{
    private readonly ILogger _logger = logger;

    private const string LifeTimeTagName = "ReaperLifetime";
    private const string ApprovalTageName = "ReaperStatus";

    /// <summary>
    /// Initializes the AzureResourceEntity with the given resourcePayload.
    /// </summary>
    /// <param name="resourcePayload">The resource payload containing the information required to initialize the entity.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task InitializeEntityAsync(ResourcePayload resourcePayload)
    {
        _logger.LogInformation("[EntityTrigger] Entity initialized for Resource Id '{resourceId}'", resourcePayload.ResourceId);

        // Check if current entity is already scheduled
        if (State.Scheduled)
        {
            _logger.LogWarning("[EntityTrigger] Entity already scheduled for Resource Id '{resourceId}'. Skipping further steps...", resourcePayload.ResourceId);
            return;
        }

        // Write entity state
        State.ResourceGroupName = resourcePayload.ResourceGroupName;
        State.ResourceId = resourcePayload.ResourceId;
        State.SubscriptionId = resourcePayload.SubscriptionId;
        State.Scheduled = false; // Will be set to false by default

        // Validate current Azure Resource Group
        _logger.LogInformation("[EntityTrigger] Start validation for Azure Resource Group eligibility for {rg}", resourcePayload.ResourceGroupName);
        if (await ValidateResourceGroupEligibility())
        {
            await PrepareResourceGroup();
            SetSchedule();
        }
    }

    /// <summary>
    /// Validates the eligibility of the current Azure Resource Group.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation. Returns true if the resource group is eligible; otherwise, false.</returns>
    private async Task<bool> ValidateResourceGroupEligibility()
    {
        try
        {
            // Call Azure Resource Service to get current resource group
            var rg = await azureResourceService.GetAzureResourceGroup(State.SubscriptionId, State.ResourceGroupName);

            // Validate the tags of the current Azure Resource Group
            if (TagHandler.CheckReaperTags(rg.Data.Tags, LifeTimeTagName))
            {
                _logger.LogInformation("[EntityTrigger] Resource Group {rg} contains the valid tags to continue execution", rg.Data.Name);
                return true;
            }

            _logger.LogWarning("[EntityTrigger] Resource Group {rg} does not contain the valid tags to continue execution", rg.Data.Name);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EntityTrigger] Failed to validate resource group’s eligibility with subscription Id {subId} and name {name} due to an exception", State.SubscriptionId, State.ResourceGroupName);
            return false;
        }
    }

    // Apply required tags to the current Azure Resource Group to approve Reaper schedule
    private async Task PrepareResourceGroup()
    {
        try
        {
            await azureResourceService.ApplyResourceGroupTags(State.SubscriptionId, State.ResourceGroupName,
                ApprovalTageName, "Approved");
            _logger.LogInformation("[EntityTrigger] Successfully applied approval tags to Id {subId} and name {name}", State.SubscriptionId, State.ResourceGroupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EntityTrigger] Failed to apply approval tags to Id {subId} and name {name} due to an exception", State.SubscriptionId, State.ResourceGroupName);
            throw;
        }
    }

    private void SetSchedule()
    {
        var entityInstanceId = Context.Id;
        var signalTime = DateTimeOffset.UtcNow.AddMinutes(3);
        var signalOptions = new SignalEntityOptions
        {
            SignalTime = signalTime
        };
        Context.SignalEntity(entityInstanceId, nameof(DeleteResourceAsync), signalOptions);
        State.Scheduled = true;
    }

    public async Task DeleteResourceAsync()
    {
        await azureResourceService.DeleteResourceGroupAsync(State.SubscriptionId, State.ResourceGroupName);
        State.Scheduled = false;
    }

    [Function(nameof(AzureResourceEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
    {
        return dispatcher.DispatchAsync<AzureResourceEntity>();
    }
}
