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
    
    public async Task InitializeEntity(ResourcePayload resourcePayload)
    {
        State.ResourceGroupName = resourcePayload.ResourceGroupName;
        State.ResourceId = resourcePayload.ResourceId;
        State.SubscriptionId = resourcePayload.SubscriptionId;
        State.Scheduled = false;
        
        _logger.LogInformation("[EntityTrigger] Entity initialized for Resource Id '{resourceId}'", resourcePayload.ResourceId);
    }
    
    private void ClearEntity()
    {
        
    }
    
    [Function(nameof(AzureResourceEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
    {
        return dispatcher.DispatchAsync<AzureResourceEntity>();
    }
}