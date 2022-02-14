using System.Threading.Tasks;
using AzureReaper.Functions.Models;

namespace AzureReaper.Functions.Interfaces;

public interface IAzureAuthProvider
{
    Task<AzureResourceResponse> GetResourceAsync(string resourceId);
    Task PatchResourceAsync(string resourceId, string tagName, string tagValue, AzureResourceResponse requestData);
}