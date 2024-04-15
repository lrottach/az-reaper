using AzureReaper.Function.Models;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureReaper.Function.Entities;

public class AzureResourceEntity
{
    [JsonPropertyName("resourcePayload")]
    public ResourcePayload? ResourceData { get; set; }

    [JsonPropertyName("scheduled")]
    public bool Scheduled { get; set; }

    public void InitializeEntity(ResourcePayload resourcePayload)
    {
        ResourceData = resourcePayload;
        Console.WriteLine($"Entity initialized for Resource Id '{resourcePayload.ResourceId}'");
    }

    [Function(nameof(AzureResourceEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
    {
        return dispatcher.DispatchAsync<AzureResourceEntity>();
    }
}