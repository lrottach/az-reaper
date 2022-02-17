using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using AzureReaper.Functions.Entities;
using AzureReaper.Functions.Interfaces;
using AzureReaper.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AzureReaper.Functions;

public static class OrchestratorFunction
{
    [FunctionName("AzureReaper_Orchestrator")]
    public static async Task RunOrchestratorAsync(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        // Receive and extract payload from EventGrid function
        EventPayload data = context.GetInput<EventPayload>();

        // Create new DurableEntity
        var entityId = new EntityId(nameof(ResourceEntity), data.EventSubject.Replace("/", ""));
        // Create new Proxy to interact with DurableEntity
        var proxy = context.CreateEntityProxy<IResourceEntity>(entityId);
        await proxy.InitializeEntityAsync(data.EventSubject, _reaperTagLifetime);

        if (await proxy.GetScheduleAsync())
        {
            log.LogWarning("Orchestrator: Entity was scheduled. Continue with execution");
            await proxy.ApplyApprovalTagAsync(_reaperTagApproval);
            return;
        }
        
        log.LogWarning("Orchestrator: Entity {EntityId} was not scheduled. Exiting Orchestrator", data.EventSubject.Replace("/", ""));
    }
}
