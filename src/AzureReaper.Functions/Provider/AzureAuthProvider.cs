using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using AzureReaper.Functions.Interfaces;
using AzureReaper.Functions.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

    private async Task<string> GetAccessTokenAsync()
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

    public async Task PatchResourceAsync(string resourceId, string tagName, string tagValue, AzureResourceResponse requestData)
    {
        string accessToken = GetAccessTokenAsync().Result;
        string requestUri = resourceId + ApiVersion;
        
        // Add new tag to existing resource data
        requestData.Tags.Add(tagName, tagValue);
        var jsonData = JsonConvert.SerializeObject(requestData);
        var requestContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _httpClient.PatchAsync(requestUri, requestContent);
    }
}