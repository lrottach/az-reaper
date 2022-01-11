using System;
using System.Net.Http;
using System.Threading.Tasks;
using AzureReaper.Functions.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

    public AzureResourceGroup(ILogger log, IHttpClientFactory factory)
    {
        _log = log;
        _client = factory.CreateClient();
    }
    
    public void CreateResource()
    {
        throw new NotImplementedException();
    }
    
    public void DeleteResource()
    {
        throw new NotImplementedException();
    }
        
    [FunctionName(nameof(AzureResourceGroup))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx, ILogger log)
        => ctx.DispatchAsync<AzureResourceGroup>(log);
}