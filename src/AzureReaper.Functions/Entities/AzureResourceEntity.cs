using AzureReaper.Interfaces;
using AzureReaper.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;

namespace AzureReaper.Entities;

public class AzureResourceEntity : TaskEntity<AzureResourceState>
{
    private readonly ILogger _logger;
    private readonly IAzureResourceService _azureResourceService;
    
    private const string LifeTimeTagName = "LifeTimeInHours";

    public AzureResourceEntity(ILogger<AzureResourceEntity> logger, IAzureResourceService azureResourceService)
    {
        _logger = logger;
        _azureResourceService = azureResourceService;
    }
    
    public async Task InitializeEntityAsync(ResourcePayload resourcePayload)
    {
        _logger.LogInformation("[EntityTrigger] Entity initialized for Resource Id '{resourceId}'", resourcePayload.ResourceId);
        
        // Check if current entity is already scheduled
        if (State.Scheduled)
        {
            _logger.LogWarning("[EntityTrigger] Entity already scheduled for Resource Id '{resourceId}'. Skipping further steps...", resourcePayload.ResourceId);
            return;
        }
        
        // Write state
        State.ResourceGroupName = resourcePayload.ResourceGroupName;
        State.ResourceId = resourcePayload.ResourceId;
        State.SubscriptionId = resourcePayload.SubscriptionId;
        State.Scheduled = true;
        
        // Validate current Azure Resource Group
        _logger.LogInformation("[EntityTrigger] Start validation for Azure Resource Group eligibility for {rg}", resourcePayload.ResourceGroupName);
        await ValidateResourceGroupEligibility();
    }
    
    // Validate the eligibility of the current Azure Resource Group
    private async Task ValidateResourceGroupEligibility()
    {
        try
        {
            // Call Azure Resource Service to get current resource group
            var response = await _azureResourceService.GetAzureResourceGroup(State.SubscriptionId, State.ResourceGroupName);

            if (response == null)
            {
                _logger.LogWarning("[EntityTrigger] No resource group found with subscription Id {subId} and name {name}", State.SubscriptionId, State.ResourceGroupName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EntityTrigger] Failed to validate resource group’s eligibility with subscription Id {subId} and name {name} due to an exception", State.SubscriptionId, State.ResourceGroupName);
            throw;
        }
    }
    
    [Function(nameof(AzureResourceEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
    {
        return dispatcher.DispatchAsync<AzureResourceEntity>();
    }
}