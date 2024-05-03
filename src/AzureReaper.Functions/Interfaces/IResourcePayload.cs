namespace AzureReaper.Interfaces;

public interface IResourcePayload
{
    public string? SubscriptionId { get; set; }
    public string? ResourceId { get; set; }
    public string? ResourceGroup { get; set; }
}