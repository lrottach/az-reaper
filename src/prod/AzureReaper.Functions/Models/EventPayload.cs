namespace AzureReaper.Functions.Models;

/// <summary>
/// Helper class to load data and send it to an Orchestrator function
/// </summary>
public class EventPayload
{
    public string EventSubject { get; set; }
}