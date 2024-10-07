using System.Text.Json.Serialization;

namespace xpd.githook.cc_lint;

public class ConventionalCommitConfig
{
    [JsonPropertyName("types")]
    public CommitTypes Types { get; set; } = null!;

    public class CommitTypes
    {
        [JsonPropertyName("refactor")]
        public object Refactor { get; set; } = null!;

        [JsonPropertyName("fix")]
        public object Fix { get; set; } = null!;

        [JsonPropertyName("feat")]
        public object Feat { get; set; } = null!;

        [JsonPropertyName("build")]
        public object Build { get; set; } = null!;

        [JsonPropertyName("chore")]
        public object Chore { get; set; } = null!;

        [JsonPropertyName("style")]
        public object Style { get; set; } = null!;

        [JsonPropertyName("test")]
        public object Test { get; set; } = null!;

        [JsonPropertyName("docs")]
        public object Docs { get; set; } = null!;

        [JsonPropertyName("perf")]
        public object Perf { get; set; } = null!;

        [JsonPropertyName("revert")]
        public object Revert { get; set; } = null!;
    }
}
