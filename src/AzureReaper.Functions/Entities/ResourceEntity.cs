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
    public async Task InitializeEntityAsync(EventPayload eventPayload)
    {
        // Set resource id
        ResourceId = eventPayload.EventSubject;
        
        // Check if the Reaper Lifetime tag was set
        // Only if this tag exists the Azure Reaper will to its thing
        if (await CheckReaperTagAsync(eventPayload.ReaperLifetimeTagName))
        {
            _log.LogInformation("Set entity status to scheduled for entity {ResourceId}", ResourceId);
            
            // Set status to scheduled
            Scheduled = true;
            return;
        }

        _log.LogWarning("Reaper lifetime tag was not found or does not contain a valid integer");
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
        
        // Check if response is valid and contains the required reaper lifetime tag
        if (ResourceResponse != null && ResourceResponse.Tags.ContainsKey(tagName))
        {
            _log.LogInformation("Found Reaper lifetime tag '{TagName}' on resource {ResourceId}", tagName, ResourceId);
            // Check if the reaper lifetime tag contains a valid integer
            if (ResourceResponse.Tags.TryGetValue(tagName, out var tagValue) && int.TryParse(tagValue, out var tagValueAsInt))
            {
                Lifetime = tagValueAsInt;
                _log.LogInformation("Reaper lifetime tag on resource {ResourceId}, contains a valid integer '{Lifetime}'", ResourceId, Lifetime);
                Entity.Current.DeleteState();
                return true;
            }
        }
        _log.LogWarning("Entity initialization failed. Resource {ResourceId} has no lifetime tag set", ResourceId);
        return false;
    }

    public async Task ApplyApprovalTagAsync(string tagName)
    {
        await _azureAuthProvider.PatchResourceAsync(ResourceId, tagName, "Approved", ResourceResponse);
        _log.LogInformation("Applied status tag '{TagName}' to current resource {ResourceId}", tagName, ResourceId);
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
