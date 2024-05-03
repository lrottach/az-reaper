using System.Text.Json.Serialization;
using AzureReaper.Interfaces;

namespace AzureReaper.Models;

public class AzureResourceState : IResourcePayload
{
    [JsonPropertyName("subscriptionId")]
    public string? SubscriptionId { get; set; }
    
    [JsonPropertyName("resourceId")]
    public string? ResourceId { get; set; }
    
    [JsonPropertyName("resourceGroupName")]
    public string? ResourceGroupName { get; set; }
    
    [JsonPropertyName("scheduled")]
    public bool Scheduled { get; set; }
}