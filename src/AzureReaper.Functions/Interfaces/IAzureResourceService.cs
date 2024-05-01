namespace AzureReaper.Interfaces;

public interface IAzureResourceService
{
    Task GetAzureResourceGroup(string subscriptionId, string resourceGroupName);
}