namespace xpd.CommitLinter.Models;

public sealed class CommitMessageConfig
{
    public MaxSubjectLength? MaxSubjectLength { get; set; }
    public ConventionalCommitConfig? ConventionalCommit { get; set; }
}
