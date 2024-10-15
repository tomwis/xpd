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
    private const string SolutionName = "solutionName";
    private string _outputPath = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        // Doing Arrange and Act in OneTimeSetUp, because these parts are the same for all the test.
        // I'm only validating different parts of Act results in different tests.
        // And 1 test can take about 3 seconds, so it's much faster to execute Act only once

        // Arrange
        _outputPath = PrepareOutputDir();
        var initHandler = GetSubject(new FileSystem(), new ProcessProvider(), new InputRequester());
        var init = new Init { Output = _outputPath, SolutionName = SolutionName };

        // Act
        _ = initHandler.Parse(init);
    }

    [Test]
    public void DotnetToolsManifestIsCreatedAndToolsAreInstalled()
    {
        // Assert
        var path = Path.Combine(_outputPath, SolutionName, ".config", "dotnet-tools.json");
        File.Exists(path).Should().BeTrue();
        var dotnetToolsManifest = path.Deserialize<DotnetToolsManifest>();
        dotnetToolsManifest.Tools.Should().ContainKey("csharpier");
        dotnetToolsManifest.Tools.Should().ContainKey("husky");
    }

    [Test]
    public void GitRepositoryIsInitialized()
    {
        // Assert
        var path = Path.Combine(_outputPath, SolutionName, ".git");
        Directory.Exists(path).Should().BeTrue();
    }

    [Test]
    public void HuskyHooksAreInstalled()
    {
        // Assert
        var huskyPath = Path.Combine(_outputPath, SolutionName, ".husky");
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

    private InitHandler GetSubject(
        IFileSystem fileSystem,
        IProcessProvider processProvider,
        IInputRequester inputRequester
    )
    {
        return new InitHandler(fileSystem, inputRequester, processProvider);
    }

    private class DotnetToolsManifest
    {
        [JsonPropertyName("tools")]
        public Dictionary<string, JsonObject> Tools { get; set; } = null!;
    }
}
