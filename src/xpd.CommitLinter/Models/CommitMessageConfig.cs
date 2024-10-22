namespace xpd.CommitLinter.Models;

internal sealed class CommitMessageConfig
{
    public MaxSubjectLength? MaxSubjectLength { get; set; }
    public ConventionalCommitConfig? ConventionalCommit { get; set; }
}
