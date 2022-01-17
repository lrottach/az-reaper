namespace AzureReaper.Functions.Interfaces;

public interface IAzureResourceGroup
{
    public bool GetSchedule();
    public void CreateResource(string resourceId);
    public void DeleteResource();
}
