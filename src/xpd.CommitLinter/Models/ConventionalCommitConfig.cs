namespace xpd.CommitLinter.Models;

public sealed class ConventionalCommitConfig
{
    public bool Enabled { get; set; }
    public List<string>? Types { get; set; }
}
