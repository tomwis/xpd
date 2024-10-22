using System.IO.Abstractions;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using xpd.Interfaces;
using xpd.Models;
using xpd.Services;
using xpd.tests.Assertions.Extensions;
using xpd.tests.Assertions.Models;
using xpd.tests.Extensions;
using PathProvider = xpd.tests.utilities.PathProvider;

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
        _outputPath = PathProvider.PrepareOutputDirForIntegrationTests(
            "InitHandlerIntegrationTests"
        );
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
        var tasks = Path.Combine(huskyPath, "task-runner.json").Deserialize<TaskRunner>().Tasks;
        tasks.Should().ContainSingle(task => IsCsharpierTask(task));
        tasks.Should().ContainSingle(task => IsBuildTask(task));
    }

    private static bool IsCsharpierTask(TaskRunnerTask task) =>
        IsPreCommitTask(task)
        && IsDotnetCommand(task)
        && task.Arguments.First().Equals("csharpier");

    private static bool IsBuildTask(TaskRunnerTask task) =>
        IsPreCommitTask(task) && IsDotnetCommand(task) && task.Arguments.First().Equals("build");

    private static bool IsDotnetCommand(TaskRunnerTask task) => task.Command.Equals("dotnet");

    private static bool IsPreCommitTask(TaskRunnerTask task) => task.Group.Equals("pre-commit");

    [Test]
    public void TestProjectHasAllNugetsInstalled()
    {
        // Arrange
        const string testProjectName = $"{SolutionName}.Tests";
        var csprojPath = Path.Combine(
            _outputPath,
            SolutionName,
            "tests",
            testProjectName,
            $"{testProjectName}.csproj"
        );
        var project = XDocument.Load(csprojPath);

        // Assert
        project
            .Root!.Descendants("PackageReference")
            .Select(e => e.Attribute("Include")!.Value)
            .Should()
            .Contain(
                "FluentAssertions",
                "NSubstitute",
                "NSubstitute.Analyzers.CSharp",
                "AutoFixture",
                "AutoFixture.AutoNSubstitute"
            );
    }

    [Test]
    public void ReadmeIsCreatedInMainFolder()
    {
        // Arrange
        string readmePath = Path.Combine(_outputPath, SolutionName, "README.md");

        // Assert
        readmePath.ToFile().Exists.Should().BeTrue();
        var readmeContent = File.ReadAllText(readmePath);
        readmeContent.Should().Contain("## Project Initialization");
    }

    [Test]
    public void EditorconfigIsCreatedInMainFolder()
    {
        // Arrange
        string filePath = Path.Combine(_outputPath, SolutionName, ".editorconfig");

        // Assert
        filePath.ToFile().Exists.Should().BeTrue();
        var readmeContent = File.ReadAllText(filePath);
        readmeContent.Should().Contain("root = true");
    }

    [Test]
    public void SolutionFolderIsAddedToSlnFile()
    {
        // Arrange
        string slnPath = Path.Combine(_outputPath, SolutionName, $"{SolutionName}.sln");

        // Assert

        // Act & Assert
        var slnContent = File.ReadAllText(slnPath);
        new SolutionFileForTest(slnContent)
            .Should()
            .HaveSolutionFolder("SolutionSettings")
            .Which.Should()
            .HaveItem("Directory.Build.targets", "Directory.Build.targets")
            .And.HaveItem("Directory.Packages.props", "Directory.Packages.props")
            .And.HaveItem("task-runner.json", ".husky/task-runner.json")
            .And.HaveItem(".gitignore", ".gitignore")
            .And.HaveItem(".editorconfig", ".editorconfig");
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
