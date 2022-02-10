using System.Text.RegularExpressions;

namespace AzureReaper.Functions.Common;

public static class StringHandler
{
    public static string ExtractResourceGroupName(string resourceId)
    {
        Regex regex = new Regex(@"\/resourceGroups\/(.*)", RegexOptions.IgnoreCase);
        MatchCollection match = regex.Matches(resourceId);
        return match[0].Groups[1].Value;
    }
}