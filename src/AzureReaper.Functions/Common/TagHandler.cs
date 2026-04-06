namespace AzureReaper.Common;

public static class TagHandler
{
    public static bool TryGetLifetimeMinutes(IDictionary<string, string> tags, string tagName, out int lifetimeMinutes)
    {
        ArgumentNullException.ThrowIfNull(tags);

        if (tags.TryGetValue(tagName, out var tagValue)
            && int.TryParse(tagValue, out lifetimeMinutes)
            && lifetimeMinutes > 0)
        {
            return true;
        }

        lifetimeMinutes = 0;
        return false;
    }
}
