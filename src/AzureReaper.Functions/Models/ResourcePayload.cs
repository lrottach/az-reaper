using AzureReaper.Interfaces;

namespace AzureReaper.Models;

// Represents the information required to initialize a new AzureResourceEntity
public class ResourcePayload : IResourcePayload
{
    public required string? SubscriptionId { get; set; }
    public required string? ResourceId { get; set; }
    public required string? ResourceGroupName { get; set; }
}
