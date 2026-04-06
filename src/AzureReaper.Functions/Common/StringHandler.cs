using Azure.Core;
using AzureReaper.Models;

namespace AzureReaper.Common;

public static class StringHandler
{
    // Extract required information from an Azure resource ID and return a ResourcePayload object
    public static ResourcePayload ExtractResourcePayload(string resourceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);

        var rid = new ResourceIdentifier(resourceId);
        return new ResourcePayload
        {
            SubscriptionId = rid.SubscriptionId,
            ResourceId = resourceId,
            ResourceGroupName = rid.ResourceGroupName
        };
    }
}
