using System.Threading.Tasks;

namespace AzureReaper.Functions.Interfaces;

public interface IResourceEntity
{
    public bool GetSchedule();
    public Task CreateResource(string resourceId);
    public void DeleteResource();
    public Task CheckReaperTag();
}
