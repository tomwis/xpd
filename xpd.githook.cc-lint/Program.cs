using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommandLine;

namespace xpd.githook.cc_lint;

public class Program
{
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

    const int HeaderMaxLength = 90;

    private static void Run(string commitMessageFileName, string conventionalCommitOptionsFileName)
    {
        var commitMessage = File.ReadAllLines(commitMessageFileName);
        var commitHeader = commitMessage[0];
        for (int i = 0; i < commitMessage.Length; ++i)
        {
            Console.WriteLine($"{i}: {commitMessage[i]}");
        }

        if (commitHeader.Length > HeaderMaxLength)
        {
            throw new CommitFormatException(
                $"Commit title too long ({commitHeader.Length} characters). Should have max {HeaderMaxLength}."
            );
        }

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

        var commitType = commitHeader.Split(':')[0].Split('(')[0];

        if (names.All(t => t != commitType))
        {
            throw new CommitFormatException(
                $"Commit type is not on accepted list. Current: {commitType}. Should be one of: {string.Join(", ", names)}"
            );
        }

        var commitSubject = commitHeader.Split(':')[1];
        if (string.IsNullOrWhiteSpace(commitSubject))
        {
            throw new CommitFormatException("Commit subject cannot be empty.");
        }

        var lines = commitMessage.Length;
        Console.WriteLine($"lines: {lines}");
        if (lines == 1)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(commitMessage[1]))
        {
            throw new CommitFormatException("There must be blank line between header and body.");
        }

        if (lines >= 3 && string.IsNullOrWhiteSpace(commitMessage[2]))
        {
            throw new CommitFormatException("Body cannot be empty.");
        }
    }
}
