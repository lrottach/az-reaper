using AzureReaper.Interfaces;

namespace AzureReaper.Models;

public class AzureResourceState : IResourcePayload
{
    public string? SubscriptionId { get; set; }
    public string? ResourceId { get; set; }
    public string? ResourceGroup { get; set; }
    public bool Scheduled { get; set; }
}