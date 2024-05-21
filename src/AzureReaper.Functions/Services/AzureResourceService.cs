using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureReaper.Interfaces;

namespace AzureReaper.Services;

public class AzureResourceService : IAzureResourceService
{
    private DefaultAzureCredential Credential { get; } = new();
    
    public async Task GetAzureResourceGroup(string? subscriptionId, string? resourceGroupName)
    {
        var client = new ArmClient(Credential);
        var resourceGroupIdentifier = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
        var resourceGroupResource = client.GetResourceGroupResource(resourceGroupIdentifier);
        var resourceGroup = await resourceGroupResource.GetAsync();
        Console.WriteLine($"[] Resource Group '{resourceGroup.Value.Data.Name}' found");
    }
}
