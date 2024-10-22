using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.Json;
using xpd.CommitLinter.Models;

namespace xpd.CommitLinter;

public class Linter
{
    private const char Checkmark = '\u2714';
    private const string SubjectSeparator = ": ";

    public void Run(IFileInfo commitMessageFile, IFileInfo commitMessageConfigFile)
    {
        var commitMessage = File.ReadAllLines(commitMessageFile.FullName);
        var commitMessageConfigContent = File.ReadAllText(commitMessageConfigFile.FullName);
        var commitMessageConfig = JsonSerializer.Deserialize<CommitMessageConfigRoot>(
            commitMessageConfigContent,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower }
        );
        var commitSubject = commitMessage[0];
        PrintCommit(commitMessage);
        SubjectLengthValidation(commitSubject, commitMessageConfig?.Config?.MaxSubjectLength);
        TypeValidation(commitSubject, commitMessageConfig?.Config?.ConventionalCommit);
        DescriptionNotEmptyValidation(commitSubject);

        var lines = commitMessage.Length;
        if (lines == 1)
        {
            Console.WriteLine("1 line commit. Checks finished.");
            return;
        }

        BlankLineBetweenSubjectAndBodyValidation(commitMessage);
        BodyNotEmptyValidation(lines, commitMessage);
    }

    private static void PrintCommit(string[] commitMessage)
    {
        Console.WriteLine("Commit message with line numbers:");
        for (int i = 0; i < commitMessage.Length; ++i)
        {
            Console.WriteLine($"    {i}: {commitMessage[i]}");
        }

        Console.WriteLine();
    }

    private static void SubjectLengthValidation(
        string commitSubject,
        MaxSubjectLength? maxSubjectLength
    )
    {
        if (maxSubjectLength is null || !maxSubjectLength.Enabled)
        {
            Console.WriteLine("Max subject length check disabled.");
            return;
        }

        if (commitSubject.Length > maxSubjectLength.Value)
        {
            throw new CommitFormatException(
                $"Commit subject is too long ({commitSubject.Length} characters). Should have max {maxSubjectLength.Value} characters."
            );
        }

        Console.WriteLine(
            $"Subject length check passed ({commitSubject.Length}/{maxSubjectLength.Value} characters) {Checkmark}"
        );
    }

    private static void TypeValidation(
        string commitSubject,
        ConventionalCommitConfig? conventionalCommitConfig
    )
    {
        if (conventionalCommitConfig?.Types is null || !conventionalCommitConfig.Enabled)
        {
            Console.WriteLine("Conventional commit check disabled.");
            return;
        }

        var allowedTypes = conventionalCommitConfig
            .Types.Select(p => p.ToLowerInvariant())
            .ToList();

        var commitType = commitSubject.Split(SubjectSeparator)[0].Split('(')[0];
        if (allowedTypes.All(t => t != commitType))
        {
            throw new CommitFormatException(
                $"Commit type is not on accepted list. Current: {commitType}. Should be one of: {string.Join(", ", allowedTypes)}"
            );
        }

        Console.WriteLine($"Type ({commitType}) check passed {Checkmark}");
    }

    private static void DescriptionNotEmptyValidation(string commitSubject)
    {
        var commitDescription = commitSubject.Split(SubjectSeparator)[1];
        if (string.IsNullOrWhiteSpace(commitDescription))
        {
            throw new CommitFormatException("Commit description cannot be empty.");
        }

        Console.WriteLine($"Description not empty check passed {Checkmark}");
    }

    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    private static void BlankLineBetweenSubjectAndBodyValidation(string[] commitMessage)
    {
        if (!string.IsNullOrWhiteSpace(commitMessage[1]))
        {
            throw new CommitFormatException("There must be blank line between header and body.");
        }

        Console.WriteLine($"Blank line between subject and body check passed {Checkmark}");
    }

    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    private static void BodyNotEmptyValidation(int lines, string[] commitMessage)
    {
        if (lines >= 3 && string.IsNullOrWhiteSpace(commitMessage[2]))
        {
            throw new CommitFormatException("Body cannot be empty.");
        }

        Console.WriteLine($"Body not empty check passed {Checkmark}");
    }
}
