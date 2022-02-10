using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzureReaper.Functions.Interfaces;

public interface IEntityFactory
{
    Task<bool> CheckEntityStatusAsync(EntityId entityId, IDurableClient client, CancellationToken cancellationToken);
}