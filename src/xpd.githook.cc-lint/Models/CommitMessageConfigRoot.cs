using System.Text.Json.Serialization;

namespace xpd.githook.cc_lint.Models;

internal sealed class CommitMessageConfigRoot
{
    [JsonPropertyName("config")]
    public CommitMessageConfig? Config { get; set; }
}
