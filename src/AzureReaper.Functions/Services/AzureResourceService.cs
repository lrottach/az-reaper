using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureReaper.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureReaper.Services;

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
}
