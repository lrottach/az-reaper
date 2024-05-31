using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureReaper.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureReaper.Services;

// Docs: https://learn.microsoft.com/en-us/dotnet/azure/sdk/resource-management?tabs=PowerShell#putting-it-all-together

public class AzureResourceService : IAzureResourceService
{
    private DefaultAzureCredential Credential { get; } = new();
    private readonly ILogger _logger;

    public AzureResourceService(ILogger<AzureResourceService> logger)
    {
        _logger = logger;
    }
    
    public async Task<ResourceGroupResource> GetAzureResourceGroup(string? subscriptionId,
        string? resourceGroupName)
    {
        var client = new ArmClient(Credential);
        var resourceGroupIdentifier = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
        var resourceGroupResource = client.GetResourceGroupResource(resourceGroupIdentifier);
        var resourceGroup = await resourceGroupResource.GetAsync();
        _logger.LogInformation("[AzureResourceService] Resource Group '{resourceGroup}' found", resourceGroup.Value.Data.Name);
        return resourceGroup.Value;
    }

    // Set the required tags to the current Azure Resource Group
    public async Task ApplyResourceGroupTags(string? subscriptionId, string? resourceGroupName, string? tagName, string? tagValue)
    {
        var rg = await GetAzureResourceGroup(subscriptionId, resourceGroupName);
        await rg.AddTagAsync(tagName, tagValue);
        _logger.LogInformation("[AzureResourceService] Tags applied to Resource Group '{resourceGroup}'", resourceGroupName);
    }

    public async Task DeleteResourceGroupAsync(string? subscriptionId, string? resourceGroupName)
    {
        var rg = await GetAzureResourceGroup(subscriptionId, resourceGroupName);
        await rg.DeleteAsync(WaitUntil.Started);
        _logger.LogInformation("[AzureResourceService] Resource Group '{resourceGroup}' deleted", resourceGroupName);
    }
}
