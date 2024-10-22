using System.Text.Json;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using xpd.Constants;
using xpd.Models;
using xpd.tests.utilities;

namespace xpd.convention_tests;

public class HuskyTaskRunnerTests
{
    [Test]
    public void ArtifactsDirNameShouldBeTheSameInCommitLinterProjectAndTaskRunner()
    {
        const string artifactsDirPropertyName = "ArtifactsDir";
        var rootFolder = PathProvider.GetRootRepoFolder();
        var csprojPath = GetCommitLinterCsprojPath(rootFolder);
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
    public void ArtifactsDirPathInCommitLinterCsprojShouldPointToArtifactsDirMainFolder()
    {
        const string artifactsDirPropertyName = "ArtifactsDir";
        var rootFolder = PathProvider.GetRootRepoFolder();
        var csprojPath = GetCommitLinterCsprojPath(rootFolder);
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
    public void PathToCommitConfigShouldBeCorrect()
    {
        const string commitMessageLinterTaskName = "commit-message-linter";
        const string commitMessageConfigJson = "commit-message-config.json";
        var rootFolder = PathProvider.GetRootRepoFolder();
        var taskRunnerPath = Path.Combine(rootFolder, ".husky", "task-runner.json");
        var json = File.ReadAllText(taskRunnerPath);
        var taskRunner = JsonSerializer.Deserialize<TaskRunner>(json);
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

    [Test]
    public void CommitLinterDllNameInTaskRunnerMustBeCorrect()
    {
        var rootFolder = PathProvider.GetRootRepoFolder();
        var taskRunner = GetTaskRunner(rootFolder);
        var commitMessageLinterTask = taskRunner!.Tasks.Single(t =>
            t.Name == "commit-message-linter"
        );
        var dllPath = commitMessageLinterTask.Arguments.First();
        var dllName = dllPath.Split(Path.DirectorySeparatorChar)[1];

        dllName.Should().Be("xpd.CommitLinter.dll");
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

    private static string GetCommitLinterCsprojPath(string rootFolder)
    {
        var csprojPath = Path.Combine(
            rootFolder,
            OptionalFoldersConstants.SrcDir,
            "xpd.CommitLinter",
            "xpd.CommitLinter.csproj"
        );
        return csprojPath;
    }
}
