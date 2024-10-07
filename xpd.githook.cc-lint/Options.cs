using CommandLine;

namespace xpd.githook.cc_lint;

public class Options
{
    [Option(
        'c',
        "commit-file",
        Required = true,
        HelpText = "Provide file with commit message to validate."
    )]
    public string CommitMessageFileName { get; set; }

    [Option(
        'o',
        "cc-options-file",
        Required = true,
        HelpText = "Provide file name of conventional commit options in json format."
    )]
    public string ConventionalCommitOptionsFileName { get; set; }
}
