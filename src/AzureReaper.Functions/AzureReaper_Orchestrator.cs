using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using AzureReaper.Functions.Entities;
using AzureReaper.Functions.Interfaces;
using AzureReaper.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AzureReaper.Functions;

public class OrchestratorFunction
{
    [FunctionName("AzureReaper_Orchestrator")]
    public static async Task RunOrchestratorAsync(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        // Receive and extract payload from EventGrid function
        EventPayload data = context.GetInput<EventPayload>();
        log.LogInformation("Started new orchestrator instance for Azure Resource: {ResourceId}", data.EventSubject);

        // Create new DurableEntity
        var entityId = new EntityId(nameof(ResourceEntity), data.EventSubject.Replace("/", ""));
        // Create new Proxy to interact with DurableEntity
        var proxy = context.CreateEntityProxy<IResourceEntity>(entityId);
        await proxy.CreateAsync(data.EventSubject);

        if (await proxy.GetScheduleAsync())
        {
            return;
        }
        
        log.LogInformation("Entity {EntityId} was not scheduled. Exiting Orchestrator", data.EventSubject.Replace("/", ""));
    }
}
