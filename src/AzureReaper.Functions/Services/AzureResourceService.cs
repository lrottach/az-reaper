using Azure;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureReaper.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureReaper.Services;

public class AzureResourceService(ArmClient armClient, ILogger<AzureResourceService> logger) : IAzureResourceService
{
    public async Task<ResourceGroupResource?> GetAzureResourceGroup(string subscriptionId,
        string resourceGroupName)
    {
        var resourceGroupIdentifier = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
        var resourceGroupResource = armClient.GetResourceGroupResource(resourceGroupIdentifier);

        try
        {
            var resourceGroup = await resourceGroupResource.GetAsync();
            logger.LogInformation("[AzureResourceService] Resource Group '{resourceGroup}' found", resourceGroup.Value.Data.Name);
            return resourceGroup.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            logger.LogWarning("[AzureResourceService] Resource Group '{resourceGroup}' not found", resourceGroupName);
            return null;
        }
    }

    public async Task ApplyResourceGroupTags(string subscriptionId, string resourceGroupName, string tagName, string tagValue)
    {
        var resourceGroupIdentifier = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
        var rg = armClient.GetResourceGroupResource(resourceGroupIdentifier);
        await rg.AddTagAsync(tagName, tagValue);
        logger.LogInformation("[AzureResourceService] Tags applied to Resource Group '{resourceGroup}'", resourceGroupName);
    }

    public async Task DeleteResourceGroupAsync(string subscriptionId, string resourceGroupName)
    {
        var resourceGroupIdentifier = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
        var rg = armClient.GetResourceGroupResource(resourceGroupIdentifier);
        await rg.DeleteAsync(WaitUntil.Started);
        logger.LogInformation("[AzureResourceService] Resource Group '{resourceGroup}' deletion started", resourceGroupName);
    }
}
