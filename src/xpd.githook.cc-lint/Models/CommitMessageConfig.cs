namespace xpd.githook.cc_lint.Models;

internal sealed class CommitMessageConfig
{
    public MaxSubjectLength? MaxSubjectLength { get; set; }
    public ConventionalCommitConfig? ConventionalCommit { get; set; }
}
