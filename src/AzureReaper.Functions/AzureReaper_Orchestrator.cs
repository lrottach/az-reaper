using System;
using System.Threading;
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
        await proxy.InitializeEntityAsync(data);

        // Continue only if the entity was successfully scheduled during initialization
        if (await proxy.GetScheduleAsync())
        {
            await proxy.ApplyApprovalTagAsync(data.ReaperApprovalTagName);
            
            // Get lifetime from entity and schedule timer
            int lifetime = await proxy.GetLifetime();
            DateTime deadline = context.CurrentUtcDateTime.AddMinutes(lifetime);
            await context.CreateTimer(deadline, CancellationToken.None);
            await proxy.DeleteResource();
            log.LogInformation("Finished execution");
            return;
        }
        
        log.LogWarning("Orchestrator: Entity {EntityId} was not scheduled. Exiting Orchestrator", data.EventSubject.Replace("/", ""));
    }
}
