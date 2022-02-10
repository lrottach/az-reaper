// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName=AzureReaper_EventGridTrigger

using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using AzureReaper.Functions.Entities;
using AzureReaper.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace AzureReaper.Functions;

public class EventGridTrigger
{
    /// <summary>
    /// Azure Function triggered by Event Grid
    /// </summary>
    [FunctionName("AzureReaper_EventGridTrigger")]
    public async Task RunAsync([EventGridTrigger] EventGridEvent eventGridEvent,
        [DurableClient] IDurableClient client,
        ILogger log)
    {
        // Ensure this function only runs for the correct event type
        if (eventGridEvent.EventType != "Microsoft.Resources.ResourceWriteSuccess")
        {
            log.LogError("You shall not pass: {EventType}", eventGridEvent.EventType);
            return;
        }
        
        log.LogInformation("Starting execution for Resource Group write event: {EventSubject}", eventGridEvent.Subject);
        
        // Check if entity already exists and Reaper is already scheduled
        // Orchestrator wont run if entity already exists
        var entityId = new EntityId(nameof(ResourceEntity), eventGridEvent.Subject.Replace("/", ""));
        var entityState = await client.ReadEntityStateAsync<ResourceEntity>(entityId);
        if (entityState.EntityExists && entityState.EntityState.Scheduled)
        {
            log.LogWarning("Entity for Resource Id '{ResourceId}' already exists and death was already scheduled", entityState.EntityState.ResourceId);
            return;
        }

        // Initialize orchestrator to interact with Durable Entities
        string instanceId = await client.StartNewAsync("AzureReaper_Orchestrator", null, new EventPayload{EventSubject = eventGridEvent.Subject});
        log.LogInformation("Started orchestration with Id: {InstanceId}", instanceId);
    }
}
