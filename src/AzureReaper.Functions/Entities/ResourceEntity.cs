using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using AzureReaper.Functions.Interfaces;
using AzureReaper.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureReaper.Functions.Entities;

[JsonObject(MemberSerialization.OptIn)]
public class ResourceEntity : IResourceEntity
{
    private readonly ILogger _log;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAzureAuthProvider _azureAuthProvider;
    
    [JsonProperty("scheduledDeath")]
    public DateTime ScheduledDeath { get; set; }
    
    [JsonProperty("resourceId")]
    public string ResourceId { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("scheduled")]
    public bool Scheduled { get; set; }

    [JsonProperty("resourceGroup")]
    public AzureResourceResponse Resource { get; set; }
    
    public ResourceEntity(IHttpClientFactory httpClientFactory, ILogger log, IAzureAuthProvider azureAuthProvider)
    {
        _azureAuthProvider = azureAuthProvider;
        _httpClientFactory = httpClientFactory;
        _log = log;
    }

    public bool GetSchedule()
    {
        return Scheduled;
    }
    
    public async Task CreateResource(string resourceId)
    {
        ResourceId = resourceId;
        
        if (await CheckReaperIdentifierTag("Reaper_Scheduled"))
        {
            _log.LogInformation("Set status to scheduled");
            Scheduled = true;
            return;
        }

        Scheduled = false;
    }

    public async Task<bool> CheckReaperIdentifierTag(string tag)
    {
        var resourceGroupResponse = await _azureAuthProvider.GetResourceAsync(ResourceId);
        
        // Checking if the required tag was set to identify if this Azure Resource Group should be deleted or not
        if (resourceGroupResponse != null && resourceGroupResponse.Tags.TryGetValue("Reaper_Scheduled", out var tagValue) && tagValue == "true")
        {
            _log.LogInformation("Required tag is set. Continue with Reaper appointment");
            return true;
        }
        _log.LogInformation("Required tag is not set");
        return false;
    }
    
    public void DeleteResource()
    {
        throw new NotImplementedException();
    }

    public Task CheckReaperTag()
    {
        throw new NotImplementedException();
    }

    [FunctionName(nameof(ResourceEntity))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx, ILogger log)
        => ctx.DispatchAsync<ResourceEntity>(log);
}
