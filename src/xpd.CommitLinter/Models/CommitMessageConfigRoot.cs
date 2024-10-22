using System.Text.Json.Serialization;

namespace xpd.CommitLinter.Models;

public sealed class CommitMessageConfigRoot
{
    [JsonPropertyName("config")]
    public CommitMessageConfig? Config { get; set; }
}
