using AzureReaper.Interfaces;
using AzureReaper.Models;
using AzureReaper.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;

namespace AzureReaper.Entities;

public class AzureResourceEntity : TaskEntity<AzureResourceState>
{
    private readonly IAzureResourceService _azureResourceService;
    
    private const string LifeTimeTagName = "LifeTimeInHours";

    public AzureResourceEntity(IAzureResourceService azureResourceService)
    {
        _azureResourceService = azureResourceService;
    }
    
    public async Task InitializeEntity(ResourcePayload resourcePayload)
    {
        State.ResourceGroupName = resourcePayload.ResourceGroupName;
        State.ResourceId = resourcePayload.ResourceId;
        State.SubscriptionId = resourcePayload.SubscriptionId;
        
        Console.WriteLine($"[EntityTrigger] Entity initialized for Resource Id '{resourcePayload.ResourceId}'");
        // await _azureResourceService.GetAzureResourceGroup(State.SubscriptionId, State.ResourceGroupName);
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