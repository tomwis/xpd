namespace xpd.CommitLinter.Models;

internal sealed class ConventionalCommitConfig
{
    public bool Enabled { get; set; }
    public List<string>? Types { get; set; }
}
