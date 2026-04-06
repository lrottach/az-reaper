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

namespace AzureReaper;

public class EventGridTrigger(ILogger<EventGridTrigger> logger)
{
    [Function(nameof(EventGridTrigger))]
    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent,
        [DurableClient] DurableTaskClient client)
    {
        logger.LogInformation("[EventGridTrigger] Event type: {EventType}, Event subject: {Subject}", eventGridEvent.EventType, eventGridEvent.Subject);

        // Additional validation in case EventGrid subscription filters aren't configured correctly
        if (eventGridEvent.EventType != "Microsoft.Resources.ResourceWriteSuccess")
        {
            logger.LogInformation("[EventGridTrigger] Skipping unsupported event type: {EventType}", eventGridEvent.EventType);
            return;
        }

        // Filter to resource-group-level events only (not child resources like storage accounts, VMs, etc.)
        // Resource group subjects have the format: /subscriptions/{subId}/resourceGroups/{rgName}
        // Recommended: also configure subject filtering on the EventGrid subscription for efficiency
        string[] subjectParts = eventGridEvent.Subject.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (subjectParts.Length != 4 ||
            !subjectParts[0].Equals("subscriptions", StringComparison.OrdinalIgnoreCase) ||
            !subjectParts[2].Equals("resourceGroups", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogDebug("[EventGridTrigger] Skipping non-resource-group event: {Subject}", eventGridEvent.Subject);
            return;
        }

        try
        {
            // Build entity ID from subscription and resource group name
            string entityKey = $"{subjectParts[1]}_{subjectParts[3]}";
            var entityId = new EntityInstanceId(nameof(AzureResourceEntity), entityKey);

            // Build resource payload from event subject
            ResourcePayload resourcePayload = StringHandler.ExtractResourcePayload(eventGridEvent.Subject);

            // Signal entity to initialize
            await client.Entities.SignalEntityAsync(entityId, "InitializeEntityAsync", resourcePayload);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EventGridTrigger] Failed to process event: {Subject}", eventGridEvent.Subject);
            throw;
        }
    }
}