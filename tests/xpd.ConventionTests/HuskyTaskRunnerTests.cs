using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using xpd.Models;
using xpd.Tests.Utilities;

namespace xpd.ConventionTests;

public class HuskyTaskRunnerTests
{
    [Test]
    public void PathToCommitConfigShouldBeCorrect()
    {
        const string commitMessageLinterTaskName = "commit-message-linter";
        const string commitMessageConfigJson = "commit-message-config.json";
        var rootFolder = PathProvider.GetRootRepoFolder();
        var taskRunner = GetTaskRunner(rootFolder);
        var commitMessageLinterTask = taskRunner!.Tasks.Single(t =>
            t.Name == commitMessageLinterTaskName
        );
        var commitConfigPath = commitMessageLinterTask.Arguments.Single(arg =>
            arg.Contains(commitMessageConfigJson)
        );

        var expectedPath = Path.Combine(rootFolder, commitConfigPath);
        File.Exists(expectedPath)
            .Should()
            .BeTrue(
                $"Path to {commitMessageConfigJson} ({commitConfigPath}) in {commitMessageLinterTaskName} task in .husky/task-runner.json is incorrect."
            );
    }

    private static TaskRunner? GetTaskRunner(string rootFolder)
    {
        var taskRunnerPath = Path.Combine(rootFolder, ".husky", "task-runner.json");
        var taskRunnerJson = File.ReadAllText(taskRunnerPath);
        var taskRunner = JsonSerializer.Deserialize<TaskRunner>(taskRunnerJson);
        return taskRunner;
    }
}
