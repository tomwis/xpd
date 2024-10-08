using System.Text.Json;
using CommandLine;

namespace xpd.githook.cc_lint;

public class Program
{
    private const char Checkmark = '\u2714';
    private const int SubjectMaxLength = 90;

    public static void Main(string[] args)
    {
        Parser
            .Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                Run(o.CommitMessageFileName, o.ConventionalCommitOptionsFileName);
            })
            .WithNotParsed(errors => { });
    }

    private static void Run(string commitMessageFileName, string conventionalCommitOptionsFileName)
    {
        var commitMessage = File.ReadAllLines(commitMessageFileName);
        var commitSubject = commitMessage[0];
        PrintCommit(commitMessage);
        SubjectLengthValidation(commitSubject);
        TypeValidation(conventionalCommitOptionsFileName, commitSubject);
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

    private static void SubjectLengthValidation(string commitSubject)
    {
        if (commitSubject.Length > SubjectMaxLength)
        {
            throw new CommitFormatException(
                $"Commit subject is too long ({commitSubject.Length} characters). Should have max {SubjectMaxLength} characters."
            );
        }

        Console.WriteLine(
            $"Subject length check passed ({commitSubject.Length}/{SubjectMaxLength} characters) {Checkmark}"
        );
    }

    private static void TypeValidation(
        string conventionalCommitOptionsFileName,
        string commitSubject
    )
    {
        var typesFileContent = File.ReadAllText(conventionalCommitOptionsFileName);
        var ccConfig = JsonSerializer.Deserialize<ConventionalCommitConfig>(typesFileContent);
        if (ccConfig is null)
        {
            throw new JsonException("Couldn't deserialize conventional commit config.");
        }

        var names = ccConfig
            .Types.GetType()
            .GetProperties()
            .Select(p => p.Name.ToLowerInvariant())
            .ToList();

        var commitType = commitSubject.Split(':')[0].Split('(')[0];

        if (names.All(t => t != commitType))
        {
            throw new CommitFormatException(
                $"Commit type is not on accepted list. Current: {commitType}. Should be one of: {string.Join(", ", names)}"
            );
        }

        Console.WriteLine($"Type ({commitType}) check passed {Checkmark}");
    }

    private static void DescriptionNotEmptyValidation(string commitSubject)
    {
        var commitDescription = commitSubject.Split(':')[1];
        if (string.IsNullOrWhiteSpace(commitDescription))
        {
            throw new CommitFormatException("Commit description cannot be empty.");
        }

        Console.WriteLine($"Description not empty check passed {Checkmark}");
    }

    private static void BlankLineBetweenSubjectAndBodyValidation(string[] commitMessage)
    {
        if (!string.IsNullOrWhiteSpace(commitMessage[1]))
        {
            throw new CommitFormatException("There must be blank line between header and body.");
        }

        Console.WriteLine($"Blank line between subject and body check passed {Checkmark}");
    }

    private static void BodyNotEmptyValidation(int lines, string[] commitMessage)
    {
        if (lines >= 3 && string.IsNullOrWhiteSpace(commitMessage[2]))
        {
            throw new CommitFormatException("Body cannot be empty.");
        }

        Console.WriteLine($"Body not empty check passed {Checkmark}");
    }
}
