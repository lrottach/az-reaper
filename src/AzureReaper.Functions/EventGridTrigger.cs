// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using Azure.Core;
using Azure.Messaging.EventGrid;
using Azure.ResourceManager.Resources;
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

        try
        {
            // Parse the event subject using Azure SDK's ResourceIdentifier for validation and data extraction
            // Recommended: also configure subject filtering on the EventGrid subscription for efficiency
            var resourceId = new ResourceIdentifier(eventGridEvent.Subject);

            // Filter to resource-group-level events only (not child resources like storage accounts, VMs, etc.)
            // Resource group subjects have the format: /subscriptions/{subId}/resourceGroups/{rgName}
            if (resourceId.ResourceType != ResourceGroupResource.ResourceType)
            {
                logger.LogDebug("[EventGridTrigger] Skipping non-resource-group event: {Subject}", eventGridEvent.Subject);
                return;
            }

            // Build entity ID from normalized subscription and resource group name
            // Azure resource IDs are case-insensitive, so normalize to prevent duplicate entities
            string entityKey = $"{resourceId.SubscriptionId!.ToLowerInvariant()}_{resourceId.Name!.ToLowerInvariant()}";
            var entityId = new EntityInstanceId(nameof(AzureResourceEntity), entityKey);

            // Build resource payload from parsed resource identifier
            var resourcePayload = new ResourcePayload
            {
                SubscriptionId = resourceId.SubscriptionId,
                ResourceId = eventGridEvent.Subject,
                ResourceGroupName = resourceId.Name
            };

            // Signal entity to initialize
            await client.Entities.SignalEntityAsync(entityId, nameof(AzureResourceEntity.InitializeEntityAsync), resourcePayload);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EventGridTrigger] Failed to process event: {Subject}", eventGridEvent.Subject);
            throw;
        }
    }
}