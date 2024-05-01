using System.Text.Json.Serialization;
using AzureReaper.Function.Models;
using AzureReaper.Interfaces;
using AzureReaper.Services;
using Microsoft.Azure.Functions.Worker;

namespace AzureReaper.Entities;

public class AzureResourceEntity
{
    private readonly IAzureResourceService _azureResourceService = new AzureResourceService();
    
    [JsonPropertyName("resourcePayload")]
    private ResourcePayload? ResourceData { get; set; }

    [JsonPropertyName("scheduled")]
    public bool Scheduled { get; private set; }

    public async Task InitializeEntity(ResourcePayload resourcePayload)
    {
        ResourceData = resourcePayload;
        Console.WriteLine($"[EntityTrigger] Entity initialized for Resource Id '{resourcePayload.ResourceId}'");
        await _azureResourceService.GetAzureResourceGroup(ResourceData.SubscriptionId, ResourceData.ResourceGroup);
    }
    
    private void ClearEntity()
    {
        Scheduled = false;
        ResourceData = null;
        Console.WriteLine($"Entity unscheduled for Resource Id '{ResourceData?.ResourceId}'");
    }
    
    [Function(nameof(AzureResourceEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
    {
        return dispatcher.DispatchAsync<AzureResourceEntity>();
    }
}