using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;

namespace AzureReaper.Function.Entities;

[JsonObject(MemberSerialization.OptIn)]
public class AzureResourceEntity
{
    [JsonProperty("value")]
    public int CurrentValue { get; set; }

    [JsonProperty("subscriptionId")]
    public string SubscriptionId { get; set; }

    public void Add(int amount) => this.CurrentValue += amount;

    public void Reset() => this.CurrentValue = 0;

    public int Get() => this.CurrentValue;

    [Function(nameof(AzureResourceEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
    {
        return dispatcher.DispatchAsync<AzureResourceEntity>();
    }
}