using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using AzureReaper.Functions.Interfaces;
using AzureReaper.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AzureReaper.Functions.Provider;

/// <summary>
/// Class to authenticate against Azure and provide access to Azure resources
/// </summary>
public class AzureAuthProvider : IAzureAuthProvider
{
    private readonly ILogger _log;
    private readonly HttpClient _httpClient;
    private static string ApiVersion = "?api-version=2014-04-01-preview";
        
    public AzureAuthProvider(ILogger log, IHttpClientFactory httpClientFactory)
    {
        _log = log;
        _httpClient = httpClientFactory.CreateClient("AzureApiClient");
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var credential = new DefaultAzureCredential();
        var accessToken = await credential
            .GetTokenAsync(new TokenRequestContext(new[] { "https://management.core.windows.net/.default" }));
        return accessToken.Token;
    }

    public async Task<AzureResourceResponse> GetResourceAsync(string resourceId)
    {
        string accessToken = GetAccessTokenAsync().Result;
        string requestUri = resourceId + ApiVersion;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await _httpClient.GetFromJsonAsync<AzureResourceResponse>(requestUri);
    }
}