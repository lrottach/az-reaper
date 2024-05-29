// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using Azure.Messaging.EventGrid;
using AzureReaper.Common;
using AzureReaper.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using AzureReaper.Models;

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
            
        // Build resource payload from event subject
        ResourcePayload resourcePayload = StringHandler.ExtractResourcePayload(eventGridEvent.Subject);
            
        // Signal entity to initialize
        await client.Entities.SignalEntityAsync(entityId, "InitializeEntityAsync", resourcePayload);
    }
}
