// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using Azure.Messaging.EventGrid;
using AzureReaper.Function.Entities;
using AzureReaper.Function.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using AzureReaper.Function.Common;

namespace AzureReaper
{
    public class EventGridTrigger
    {
        private readonly ILogger<EventGridTrigger> _logger;

        public EventGridTrigger(ILogger<EventGridTrigger> logger)
        {
            _logger = logger;
        }

        [Function(nameof(EventGridTrigger))]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent,
        [DurableClient] DurableTaskClient client)
        {
            _logger.LogInformation("Event type: {type}, Event subject: {subject}", eventGridEvent.EventType, eventGridEvent.Subject);

            // Make sure function will only continue for the correct event types
            // Additional validation in case, EventGrid filter wasn't configured correctly
            if (eventGridEvent.EventType != "Microsoft.Resources.ResourceWriteSuccess")
            {
                _logger.LogInformation("You shall not pass: {EventType}", eventGridEvent.EventType);
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
                _logger.LogWarning("Entity for Resource Id '{ResourceId}' already exists and death was already scheduled", entityState.Id);
                return;
            }

            // Initialize entity
            ResourcePayload resourcePayload = StringHandler.ExtractResourcePayload(eventGridEvent.Subject);
            await client.Entities.SignalEntityAsync(entityId, "InitializeEntity", resourcePayload);
        }
    }
}
