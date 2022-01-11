// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName=AzureReaper_EventGridTrigger

using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
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
        log.LogInformation(eventGridEvent.Data.ToString());
        log.LogInformation(eventGridEvent.Subject);

        var entityId = _entityFactory.GetEntityIdAsync(eventGridEvent.Subject, default);

        // await client.SignalEntityAsync(
        //     new EntityId(nameof(AzureResourceGroup), eventGridEvent.Subject),
        //     nameof(AzureResourceGroup.CreateResource),
        //     eventGridEvent.Subject
        // );
    }
}