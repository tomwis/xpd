using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using NUnit.Framework;

namespace xpd.convention_tests;

public class HuskyTaskRunnerTests
{
    [Test]
    public void PathToConventionalCommitConfigShouldBeCorrect()
    {
        var rootFolder = GetRootRepoFolder();
        var taskRunnerPath = Path.Combine(rootFolder, ".husky", "task-runner.json");
        var json = File.ReadAllText(taskRunnerPath);
        var taskRunner = JsonSerializer.Deserialize<TaskRunner>(json);
        var commitMessageLinterTask = taskRunner!.Tasks.Single(t =>
            t.Name == "commit-message-linter"
        );
        var conventionalCommitConfigPath = commitMessageLinterTask.Args.Single(arg =>
            arg.Contains("conventionalcommit.json")
        );

        var expectedPath = Path.Combine(rootFolder, conventionalCommitConfigPath);
        File.Exists(expectedPath)
            .Should()
            .BeTrue(
                $"Path to conventionalcommit.json ({conventionalCommitConfigPath}) in commit-message-linter task in .husky/task-runner.json is incorrect."
            );
    }

    private static string GetRootRepoFolder()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir is not null && !HasGitFolder(currentDir))
        {
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        if (currentDir is null)
        {
            throw new Exception("Could not find root repo folder.");
        }

        // Make additional checks for common files to make sure it is root repo folder
        var packagesPropsExists = new FileInfo(
            Path.Combine(currentDir, "Directory.Packages.props")
        ).Exists;
        var solutionExists = new DirectoryInfo(currentDir).GetFiles("*.sln").Length == 1;
        var solutionExistsInSrc =
            new DirectoryInfo(Path.Combine(currentDir, "src")).GetFiles("*.sln").Length == 1;

        if (!packagesPropsExists && !(solutionExists || solutionExistsInSrc))
        {
            throw new Exception("Root repo folder doesn't have required files.");
        }

        Console.WriteLine($"Root repo folder: {currentDir}");
        return currentDir;

        static bool HasGitFolder(string folder) =>
            Directory.EnumerateFileSystemEntries(folder).Any(f => f.EndsWith(".git"));
    }

    private class TaskRunner
    {
        [JsonPropertyName("tasks")]
        public List<Task> Tasks { get; init; } = null!;
    }

    private class Task
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        [JsonPropertyName("args")]
        public List<string> Args { get; init; } = null!;
    }
}
