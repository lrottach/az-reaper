using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureReaper.Interfaces;

namespace AzureReaper.Function.Services;

public class AzureResourceService : IAzureResourceService
{
    private DefaultAzureCredential Credential { get; } = new();
}
