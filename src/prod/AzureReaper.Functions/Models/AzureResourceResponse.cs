using System.Collections.Generic;
using Newtonsoft.Json;

namespace AzureReaper.Functions.Models;

public record AzureResourceResponse
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; }

    [JsonProperty("tags")]
    public Dictionary<string, string> Tags { get; set; }
} 

