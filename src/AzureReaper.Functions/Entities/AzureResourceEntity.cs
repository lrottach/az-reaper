using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;

namespace AzureReaper.Function.Entities;

[JsonObject(MemberSerialization.OptIn)]
public class AzureResourceEntity
{
    [JsonProperty("resourceId")]
    public string? ResourceId { get; set; }

    [JsonProperty("subscriptionId")]
    public string? SubscriptionId { get; set; }

    [JsonProperty("scheduled")]
    public bool Scheduled { get; set; }

    public void InitializeEntity()
    {

    }

    [Function(nameof(AzureResourceEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
    {
        return dispatcher.DispatchAsync<AzureResourceEntity>();
    }
}