namespace AzureReaper.Function.Models;

// Represents the information required to initialize a new AzureResourceEntity
public class ResourcePayload
{
    public required string SubscriptionId { get; set; }
    public string? ResourceId { get; set; }
    public required string ResourceGroup { get; set; }
}
