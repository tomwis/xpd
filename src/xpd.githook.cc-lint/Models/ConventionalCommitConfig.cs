namespace xpd.githook.cc_lint.Models;

internal sealed class ConventionalCommitConfig
{
    public bool Enabled { get; set; }
    public List<string>? Types { get; set; }
}
