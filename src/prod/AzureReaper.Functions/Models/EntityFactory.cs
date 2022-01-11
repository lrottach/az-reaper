using System.Threading;
using System.Threading.Tasks;
using AzureReaper.Functions.Interfaces;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureReaper.Functions.Models;

public class EntityFactory : IEntityFactory
{
    public Task<EntityId> GetEntityIdAsync(string resourceId, CancellationToken token)
    {
        EntityId entityId = new EntityId(nameof(AzureResourceGroup), resourceId);
        return Task.FromResult(entityId);
    }
}