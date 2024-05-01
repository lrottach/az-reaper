// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using Azure.Messaging.EventGrid;
using AzureReaper.Entities;
using AzureReaper.Function.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using AzureReaper.Function.Common;

namespace AzureReaper
{
    public class EventGridTrigger(ILogger<EventGridTrigger> logger)
    {
        [Function(nameof(EventGridTrigger))]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent,
        [DurableClient] DurableTaskClient client)
        {
            logger.LogInformation("[EventGridTrigger] Event type: {type}, Event subject: {subject}", eventGridEvent.EventType, eventGridEvent.Subject);

            // Make sure function will only continue for the correct event types
            // Additional validation in case, EventGrid filter wasn't configured correctly
            if (eventGridEvent.EventType != "Microsoft.Resources.ResourceWriteSuccess")
            {
                logger.LogInformation("[EventGridTrigger] You shall not pass: {EventType}", eventGridEvent.EventType);
                return;
            }

            // Create new entity for current event
            EntityInstanceId entityId = new EntityInstanceId(nameof(AzureResourceEntity), eventGridEvent.Subject.Replace("/", ""));

            // Read entities state to process later
            EntityMetadata<AzureResourceEntity>? entityState = await client.Entities.GetEntityAsync<AzureResourceEntity>(entityId);

            // If entity exists and is scheduled, nothing to do
            // if (entityState != null || entityState.State.Scheduled)
            if (entityState != null)
            {
                if (entityState.State.Scheduled)
                {
                    logger.LogWarning("[EventGridTrigger] Entity for Resource Id '{ResourceId}' was already scheduled", entityState.Id);
                    return;
                }
                
                logger.LogWarning("[EventGridTrigger] Entity for Resource Id '{ResourceId}' already exists, but death has not been scheduled", entityState.Id);
            }

            // Initialize entity
            ResourcePayload resourcePayload = StringHandler.ExtractResourcePayload(eventGridEvent.Subject);
            await client.Entities.SignalEntityAsync(entityId, "InitializeEntity", resourcePayload);
        }
    }
}
