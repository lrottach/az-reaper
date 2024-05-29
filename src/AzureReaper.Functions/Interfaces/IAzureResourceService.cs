using Azure;
using Azure.ResourceManager.Resources;

namespace AzureReaper.Interfaces;

public interface IAzureResourceService
{
    Task<ResourceGroupResource> GetAzureResourceGroup(string? subscriptionId, string? resourceGroupName);
}