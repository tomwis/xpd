using System.IO.Abstractions;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FluentAssertions;
using NUnit.Framework;
using xpd.Interfaces;
using xpd.Models;
using xpd.Services;
using xpd.tests.Extensions;
using xpd.tests.utilities;

namespace xpd.tests.IntegrationTests;

public class InitHandlerIntegrationTests
{
    [Test]
    public void WhenInitParseIsCalled_ThenDotnetToolsManifestIsCreatedAndToolsAreInstalled()
    {
        // Arrange
        const string solutionName = "solutionName";
        var outputPath = PrepareOutputDir();
        var initHandler = GetSubject(new FileSystem(), new ProcessProvider(), new InputRequestor());
        var init = new Init { Output = outputPath, SolutionName = solutionName };

        // Act
        _ = initHandler.Parse(init);

        // Assert
        var path = Path.Combine(outputPath, solutionName, ".config", "dotnet-tools.json");
        File.Exists(path).Should().BeTrue();
        var dotnetToolsManifest = path.Deserialize<DotnetToolsManifest>();
        dotnetToolsManifest.Tools.Should().ContainKey("csharpier");
        dotnetToolsManifest.Tools.Should().ContainKey("husky");
    }

    [Test]
    public void WhenInitParseIsCalled_ThenGitRepositoryIsInitialized()
    {
        // Arrange
        const string solutionName = "solutionName";
        var outputPath = PrepareOutputDir();
        var initHandler = GetSubject(new FileSystem(), new ProcessProvider(), new InputRequestor());
        var init = new Init { Output = outputPath, SolutionName = solutionName };

        // Act
        _ = initHandler.Parse(init);

        // Assert
        var path = Path.Combine(outputPath, solutionName, ".git");
        Directory.Exists(path).Should().BeTrue();
    }

    [Test]
    public void WhenInitParseIsCalled_ThenHuskyHooksAreInstalled()
    {
        // Arrange
        const string solutionName = "solutionName";
        var outputPath = PrepareOutputDir();
        var initHandler = GetSubject(new FileSystem(), new ProcessProvider(), new InputRequestor());
        var init = new Init { Output = outputPath, SolutionName = solutionName };

        // Act
        _ = initHandler.Parse(init);

        // Assert
        var huskyPath = Path.Combine(outputPath, solutionName, ".husky");
        Path.Combine(huskyPath, "task-runner.json").ToFile().Exists.Should().BeTrue();
        Path.Combine(huskyPath, "pre-commit").ToFile().Exists.Should().BeTrue();
        Path.Combine(huskyPath, "task-runner.json")
            .Deserialize<TaskRunner>()
            .Tasks.SingleOrDefault(IsCsharpierTask)
            .Should()
            .NotBeNull();

        bool IsCsharpierTask(TaskRunnerTask task) => task.Arguments.Contains("csharpier");
    }

    private static string PrepareOutputDir()
    {
        const string outputDir = "XpdIntegrationTestsOutputDir";
        var rootRepoFolder = PathProvider.GetRootRepoFolder();
        var outputPath = Path.Combine(rootRepoFolder, "..", outputDir);
        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }

        return outputPath;
    }

    public class DotnetToolsManifest
    {
        [JsonPropertyName("tools")]
        public Dictionary<string, JsonObject> Tools { get; set; } = null!;
    }

    private InitHandler GetSubject(
        IFileSystem fileSystem,
        IProcessProvider processProvider,
        IInputRequestor inputRequestor
    )
    {
        return new InitHandler(fileSystem, inputRequestor, processProvider);
    }
}
