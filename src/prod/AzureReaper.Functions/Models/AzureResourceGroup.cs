using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Core;
using AzureReaper.Functions.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Identity;

namespace AzureReaper.Functions.Models;

[JsonObject(MemberSerialization.OptIn)]
public class AzureResourceGroup : IAzureResourceGroup
{
    private readonly ILogger _log;
    private readonly HttpClient _client;
    
    [JsonProperty("scheduledDeath")]
    public DateTime ScheduledDeath { get; set; }
    
    [JsonProperty("resourceId")]
    public string ResourceId { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("scheduled")]
    public bool Scheduled { get; set; }

    public AzureResourceGroup(HttpClient client, ILogger log)
    {
        _log = log;
        _client = client;
    }

    public bool GetSchedule()
    {
        return Scheduled;
    }
    
    public void CreateResource(string resourceId)
    {
        ResourceId = resourceId;
        Scheduled = true;
        _log.LogInformation("Set status to scheduled");
    }

    public async Task<bool> CheckReaperIdentifierTag(string tag)
    {
        var credential = new DefaultAzureCredential();
        var token = await credential.GetTokenAsync(new TokenRequestContext(new []{"https://management.core.windows.net/.default"}));

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        var response = await httpClient.DeleteAsync(
            "https://management.azure.com/subscriptions/e7933ac0-8efa-46fa-9d1e-929ae1dd5e24/resourceGroups/test1?api-version=2014-04-01-preview");

        return false;
    }
    
    public void DeleteResource()
    {
        throw new NotImplementedException();
    }
        
    [FunctionName(nameof(AzureResourceGroup))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx, ILogger log)
        => ctx.DispatchAsync<AzureResourceGroup>(log);
}
