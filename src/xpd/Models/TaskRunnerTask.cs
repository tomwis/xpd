using System.Text.Json.Serialization;

namespace xpd.Models;

public class TaskRunnerTask
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    [JsonPropertyName("group")]
    public string Group { get; init; } = null!;

    [JsonPropertyName("command")]
    public string Command { get; init; } = null!;

    [JsonPropertyName("args")]
    public IEnumerable<string> Arguments { get; init; } = null!;

    [JsonPropertyName("include")]
    public IEnumerable<string> Include { get; init; } = null!;
}
