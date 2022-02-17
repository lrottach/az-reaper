namespace AzureReaper.Functions.Models;

/// <summary>
/// Helper class to load data and send it to an Orchestrator function
/// </summary>
public record EventPayload()
{
    public string EventSubject { get; init; }
    // Tag name which will be checked while entity initialization
    public string ReaperLifetimeTagName { get; } = "Reaper_Lifetime";
    // Tag name which will be set after entity was scheduled
    public string ReaperApprovalTagName { get; } = "Reaper_Status";
}