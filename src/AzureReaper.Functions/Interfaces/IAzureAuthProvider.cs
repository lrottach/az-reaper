using System.Threading.Tasks;
using AzureReaper.Functions.Models;

namespace AzureReaper.Functions.Interfaces;

public interface IAzureAuthProvider
{
    Task<string> GetAccessTokenAsync();
    Task<AzureResourceResponse> GetResourceAsync(string resourceId);
}