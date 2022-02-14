using System.Threading.Tasks;

namespace AzureReaper.Functions.Interfaces;

public interface IResourceEntity
{
    public Task<bool> GetScheduleAsync();
    public Task CreateAsync(string resourceId);
    public Task DeleteResource();
    public Task<bool> CheckReaperTagAsync(string tag);
    public Task ApplyApprovalTagAsync(string tag);
}
