using AzureReaper.Function.Models;

namespace AzureReaper.Function.Common;

public static class StringHandler
{
    // Extract required information from an Azure resource Id and return a ResourcePayload object
    public static ResourcePayload ExtractResourcePayload(string resourceId)
    {
        string[] parts = resourceId.Split("/");
        return new ResourcePayload
        {
            SubscriptionId = parts[2],
            ResourceId = resourceId,
            ResourceGroup = parts[4]
        };
    }
}
