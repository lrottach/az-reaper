using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using Azure.Messaging.EventGrid;
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
        EventPayload data = context.GetInput<EventPayload>();
        // Console.WriteLine(data.EventSubject);
        log.LogInformation(data.EventSubject);

        var entityId = new EntityId(nameof(AzureResourceGroup), data.EventSubject.Replace("/", ""));
        context.SignalEntity(entityId, "CreateResource", data.EventSubject);
        
        bool scheduleStatus = await context.CallEntityAsync<bool>(entityId, "GetSchedule");
        Console.WriteLine(scheduleStatus);
    }
}
