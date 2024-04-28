using AzureReaper.Function.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.DurableTask;
using System.Text.Json;
using System.Text.Json.Serialization;
using DurableTask.Core.Entities;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;

namespace AzureReaper.Function.Entities;

public class AzureResourceEntity
{
    [JsonPropertyName("resourcePayload")]
    private ResourcePayload? ResourceData { get; set; }

    [JsonPropertyName("scheduled")]
    public bool Scheduled { get; set; }

    public void InitializeEntity(ResourcePayload resourcePayload)
    {
        ResourceData = resourcePayload;
        Console.WriteLine($"Entity initialized for Resource Id '{resourcePayload.ResourceId}'");
    }
    
    private void Unschedule()
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