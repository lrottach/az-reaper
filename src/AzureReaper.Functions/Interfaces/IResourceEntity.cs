using System.Threading.Tasks;

namespace AzureReaper.Functions.Interfaces;

public interface IResourceEntity
{
    public Task<bool> GetScheduleAsync();
    public Task InitializeEntityAsync(EventPayload eventPayload);
    public Task DeleteResource();
    public Task<bool> CheckReaperTagAsync(string tagName);
    public Task ApplyApprovalTagAsync(string tagName);
}
