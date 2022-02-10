using System;
using System.Threading;
using System.Threading.Tasks;
using AzureReaper.Functions.Entities;
using AzureReaper.Functions.Interfaces;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Logging;

namespace AzureReaper.Functions.Factories;

// ToDo: Try injecting IDurableClient using builder.Services.AddDurableClientFactory()

/// <summary>
/// Class to interact with Durable Entities
/// </summary>
public class EntityFactory : IEntityFactory
{
    private readonly IDurableClient _durableClient;
    private readonly ILogger<EntityFactory> _log;

    public EntityFactory(ILogger<EntityFactory> log, IDurableClientFactory durableClientFactory)
    {
        _log = log;
        _durableClient = durableClientFactory.CreateClient(new DurableClientOptions
        {
            ConnectionName = "AzureWebJobsStorage",
            TaskHub = "TestHubName",
            IsExternalClient = true
        });
    }
    
    public async Task<bool> CheckEntityStatusAsync(EntityId entityId, IDurableClient client, CancellationToken cancellationToken)
    {
        var state = await _durableClient.ReadEntityStateAsync<ResourceEntity>(entityId);

        if (state.EntityExists && state.EntityState.Scheduled)
        {
            _log.LogWarning("Entity for Resource Id '{ResourceId}' already exists and death was already scheduled", state.EntityState.ResourceId);
            return true;
        }

        return false;
    }
}