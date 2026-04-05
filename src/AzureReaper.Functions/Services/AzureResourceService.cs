using Azure;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureReaper.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureReaper.Services;

public class AzureResourceService(ArmClient armClient, ILogger<AzureResourceService> logger) : IAzureResourceService
{
    public async Task<ResourceGroupResource> GetAzureResourceGroup(string? subscriptionId,
        string? resourceGroupName)
    {
        var resourceGroupIdentifier = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
        var resourceGroupResource = armClient.GetResourceGroupResource(resourceGroupIdentifier);
        var resourceGroup = await resourceGroupResource.GetAsync();
        logger.LogInformation("[AzureResourceService] Resource Group '{resourceGroup}' found", resourceGroup.Value.Data.Name);
        return resourceGroup.Value;
    }

    public async Task ApplyResourceGroupTags(string? subscriptionId, string? resourceGroupName, string? tagName, string? tagValue)
    {
        var rg = await GetAzureResourceGroup(subscriptionId, resourceGroupName);
        await rg.AddTagAsync(tagName, tagValue);
        logger.LogInformation("[AzureResourceService] Tags applied to Resource Group '{resourceGroup}'", resourceGroupName);
    }

    public async Task DeleteResourceGroupAsync(string? subscriptionId, string? resourceGroupName)
    {
        var rg = await GetAzureResourceGroup(subscriptionId, resourceGroupName);
        await rg.DeleteAsync(WaitUntil.Started);
        logger.LogInformation("[AzureResourceService] Resource Group '{resourceGroup}' deleted", resourceGroupName);
    }
}
