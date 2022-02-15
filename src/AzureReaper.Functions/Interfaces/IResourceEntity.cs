using System.Threading.Tasks;

namespace AzureReaper.Functions.Interfaces;

public interface IResourceEntity
{
    public Task<bool> GetScheduleAsync();
    public Task InitializeEntityAsync(string resourceId, string tagName);
    public Task DeleteResource();
    public Task<bool> CheckReaperTagAsync(string tagName);
    public Task ApplyApprovalTagAsync(string tagName);
}
