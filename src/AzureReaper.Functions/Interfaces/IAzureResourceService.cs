using Azure.ResourceManager.Resources;

namespace AzureReaper.Interfaces;

public interface IAzureResourceService
{
    Task<ResourceGroupResource> GetAzureResourceGroup(string? subscriptionId, string? resourceGroupName);
    Task ApplyResourceGroupTags(string? subscriptionId, string? resourceGroupName, string? tagName, string? tagValue);
    Task DeleteResourceGroupAsync(string? subscriptionId, string? resourceGroupName);
}