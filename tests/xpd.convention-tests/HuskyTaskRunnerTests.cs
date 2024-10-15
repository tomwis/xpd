using System.Text.Json;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using xpd.Models;
using xpd.tests.utilities;

namespace xpd.convention_tests;

public class HuskyTaskRunnerTests
{
    [Test]
    public void ArtifactsDirNameShouldBeTheSameInCcLintProjectAndTaskRunner()
    {
        const string artifactsDirPropertyName = "ArtifactsDir";
        var rootFolder = PathProvider.GetRootRepoFolder();
        var csprojPath = GetCcLintCsprojPath(rootFolder);
        var artifactsDirFromCsproj = GetPropertyValueFromCsproj(
                csprojPath,
                artifactsDirPropertyName
            )
            .Split(Path.DirectorySeparatorChar)
            .Last();

        var taskRunner = GetTaskRunner(rootFolder);
        var commitMessageLinterTask = taskRunner!.Tasks.Single(t =>
            t.Name == "commit-message-linter"
        );
        var artifactsDirFromTaskRunner = commitMessageLinterTask
            .Arguments.First()
            .Split(Path.DirectorySeparatorChar)[0];

        artifactsDirFromCsproj.Should().Be(artifactsDirFromTaskRunner);
    }

    [Test]
    public void ArtifactsDirPathShouldBeCorrect()
    {
        const string artifactsDirPropertyName = "ArtifactsDir";
        var rootFolder = PathProvider.GetRootRepoFolder();
        var csprojPath = GetCcLintCsprojPath(rootFolder);
        var csprojDir = new FileInfo(csprojPath).DirectoryName!;
        var artifactsDirFromCsproj = GetPropertyValueFromCsproj(
            csprojPath,
            artifactsDirPropertyName
        );

        var artifactsDirPath = Path.GetFullPath(Path.Combine(csprojDir, artifactsDirFromCsproj));
        var artifactsParentDir = Path.GetDirectoryName(artifactsDirPath);
        artifactsParentDir.Should().Be(rootFolder);
    }

    [Test]
    public void PathToConventionalCommitConfigShouldBeCorrect()
    {
        var rootFolder = PathProvider.GetRootRepoFolder();
        var taskRunnerPath = Path.Combine(rootFolder, ".husky", "task-runner.json");
        var json = File.ReadAllText(taskRunnerPath);
        var taskRunner = JsonSerializer.Deserialize<TaskRunner>(json);
        var commitMessageLinterTask = taskRunner!.Tasks.Single(t =>
            t.Name == "commit-message-linter"
        );
        var conventionalCommitConfigPath = commitMessageLinterTask.Arguments.Single(arg =>
            arg.Contains("conventionalcommit.json")
        );

        var expectedPath = Path.Combine(rootFolder, conventionalCommitConfigPath);
        File.Exists(expectedPath)
            .Should()
            .BeTrue(
                $"Path to conventionalcommit.json ({conventionalCommitConfigPath}) in commit-message-linter task in .husky/task-runner.json is incorrect."
            );
    }

    private static TaskRunner? GetTaskRunner(string rootFolder)
    {
        var taskRunnerPath = Path.Combine(rootFolder, ".husky", "task-runner.json");
        var taskRunnerJson = File.ReadAllText(taskRunnerPath);
        var taskRunner = JsonSerializer.Deserialize<TaskRunner>(taskRunnerJson);
        return taskRunner;
    }

    private static string GetPropertyValueFromCsproj(string csprojPath, string propertyName)
    {
        var csprojXml = XDocument.Load(csprojPath);
        var artifactsDirProperty = csprojXml
            .Root!.Descendants("PropertyGroup")
            .SelectMany(pg => pg.Descendants(propertyName))
            .Single();
        return artifactsDirProperty.Value;
    }

    private static string GetCcLintCsprojPath(string rootFolder)
    {
        var csprojPath = Path.Combine(
            rootFolder,
            "src",
            "xpd.githook.cc-lint",
            "xpd.githook.cc-lint.csproj"
        );
        return csprojPath;
    }
}
