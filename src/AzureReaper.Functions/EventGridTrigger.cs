// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using Azure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.DurableTask.Client;
using DurableTask.Core.Entities;
using AzureReaper.Function.Entities;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.DurableTask.Client.Entities;

namespace AzureReaper.Function
{
    public class EventGridTrigger
    {
        private readonly ILogger<EventGridTrigger> _logger;

        public EventGridTrigger(ILogger<EventGridTrigger> logger)
        {
            _logger = logger;
        }

        [Function(nameof(EventGridTrigger))]
        public async Task Run([EventGridTrigger] CloudEvent cloudEvent,
        [DurableClient] DurableTaskClient client)
        {
            _logger.LogInformation("Event type: {type}, Event subject: {subject}", cloudEvent.Type, cloudEvent.Subject);

            // Make sure function will only continue for the correct event types
            if (cloudEvent.Type != "Microsoft.Resources.ResourceWriteSuccess")
            {
                _logger.LogInformation("You shall not pass: {EventType}", cloudEvent.Type);
                return;
            }

            // Create new entity for current event
            EntityInstanceId entityId = new EntityInstanceId(nameof(AzureResourceEntity), cloudEvent.Subject.Replace("/", ""));

            // Read entities state to process later
            EntityMetadata<AzureResourceEntity>? entityState = await client.Entities.GetEntityAsync<AzureResourceEntity>(entityId);

            // If entity exists and is scheduled, nothing to do
            if (entityState != null || entityState.State.Scheduled)
            {
                _logger.LogWarning("Entity for Resource Id '{ResourceId}' already exists and death was already scheduled", entityState.State.ResourceId);
            }

            // Initialize entity
            await client.Entities.SignalEntityAsync(entityId, "InitializeEntity");
        }
    }
}
