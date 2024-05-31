namespace AzureReaper.Common;

public static class TagHandler
{
    public static bool CheckReaperTags(IDictionary<string, string> tags, string reaperTag)
    {
        // Check if current tags contain the required AzureReaper tag
        ArgumentNullException.ThrowIfNull(tags);

        // Return true if the required AzureReaper tag exists and it's value can be parsed into a valid int
        return tags.TryGetValue(reaperTag, out var tag) && CheckReaperTagValue(tag);
    }

    private static bool CheckReaperTagValue(string tagValue)
    {
        // Check if tagValue can be parsed into a valid int
        return int.TryParse(tagValue, out _);
    }
}