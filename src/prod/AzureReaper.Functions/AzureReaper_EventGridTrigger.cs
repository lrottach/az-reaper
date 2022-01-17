// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName=AzureReaper_EventGridTrigger

using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using AzureReaper.Functions.Common;
using AzureReaper.Functions.Interfaces;
using AzureReaper.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace AzureReaper.Functions;

public class EventGridTrigger
{
    private readonly IEntityFactory _entityFactory;

    public EventGridTrigger(IEntityFactory entityFactory)
    {
        _entityFactory = entityFactory;
    }
    
    [FunctionName("AzureReaper_EventGridTrigger")]
    public async Task RunAsync([EventGridTrigger] EventGridEvent eventGridEvent,
        [DurableClient] IDurableClient client,
        ILogger log)
    {
        if (eventGridEvent.EventType != "Microsoft.Resources.ResourceWriteSuccess")
        {
            log.LogError("You shall not pass: {EventType}", eventGridEvent.EventType);
            return;
        }
        
        log.LogInformation("Starting execution for Resource Group write event: {EventSubject}", eventGridEvent.Subject);
        
        // Check if entity already exists
        var entityId = new EntityId(nameof(AzureResourceGroup), eventGridEvent.Subject.Replace("/", ""));
        var state = await client.ReadEntityStateAsync<AzureResourceGroup>(entityId);
        if (state.EntityExists && state.EntityState.Scheduled)
        {
            log.LogWarning("Entity for Resource Id '{ResourceId}' already exists and death was already scheduled", state.EntityState.ResourceId);
            return;
        }

        // Initialize orchestrator to interact with Durable Entities
        string instanceId = await client.StartNewAsync("AzureReaper_Orchestrator", null, new EventPayload{EventSubject = eventGridEvent.Subject});
        log.LogInformation("Started orchestration with Id: {InstanceId}", instanceId);
    }
}