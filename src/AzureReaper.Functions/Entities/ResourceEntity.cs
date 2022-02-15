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

    [JsonProperty("resourceResponse")]
    public AzureResourceResponse ResourceResponse { get; set; }
    
    [JsonProperty("lifetime")]
    public int Lifetime { get; set; }
    
    public ResourceEntity(IHttpClientFactory httpClientFactory, ILogger log, IAzureAuthProvider azureAuthProvider)
    {
        _azureAuthProvider = azureAuthProvider;
        _httpClientFactory = httpClientFactory;
        _log = log;
    }
    
    /// <summary>
    /// Perform initial tasks like checking the Reaper Tag and setting the Entity status to scheduled
    /// </summary>
    /// <param name="resourceId">Azure Resource Id</param>
    public async Task InitializeEntityAsync(string resourceId, string tagName)
    {
        // Set resource id
        ResourceId = resourceId;
        
        // Check if the Reaper Lifetime tag was set
        // Only if this tag exists the Azure Reaper will to its thing
        if (await CheckReaperTagAsync(tagName))
        {
            _log.LogInformation("Set entity status to scheduled for entity {EntityId}", Entity.Current.EntityId);
            // Set status to scheduled
            Scheduled = true;
            return;
        }

        Scheduled = false;
    }

    /// <summary>
    /// Checking if the required tag was set to identify if this Azure Resource Group should be deleted or not
    /// </summary>
    /// <param name="tagName">Reaper Lifetime Tag</param>
    /// <returns>bool</returns>
    public async Task<bool> CheckReaperTagAsync(string tagName)
    {
        ResourceResponse = await _azureAuthProvider.GetResourceAsync(ResourceId);
        
        if (ResourceResponse != null && ResourceResponse.Tags.TryGetValue("Reaper_Lifetime", out var tagValue) && tagValue == "60")
        {
            _log.LogInformation("Required tag '{Tag}' is set. Continue with Reaper appointment", tagName);
            return true;
        }
        _log.LogInformation("Required tag '{Tag}' is not set", tagName);
        return false;
    }

    public async Task ApplyApprovalTagAsync(string tagName)
    {
        await _azureAuthProvider.PatchResourceAsync(ResourceId, tagName, "Approved", ResourceResponse);
    }

    public async Task<bool> GetScheduleAsync()
    {
        return Scheduled;
    }
    
    public Task DeleteResource()
    {
        throw new NotImplementedException();
    }

    [FunctionName(nameof(ResourceEntity))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx, ILogger log)
        => ctx.DispatchAsync<ResourceEntity>(log);
}
