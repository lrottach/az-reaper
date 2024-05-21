using AzureReaper.Models;

namespace AzureReaper.Common;

public static class StringHandler
{
    // Extract required information from an Azure resource ID and return a ResourcePayload object
    public static ResourcePayload ExtractResourcePayload(string resourceId)
    {
        string[] parts = resourceId.Split("/");
        return new ResourcePayload
        {
            SubscriptionId = parts[2],
            ResourceId = resourceId,
            ResourceGroupName = parts[4]
        };
    }
}
