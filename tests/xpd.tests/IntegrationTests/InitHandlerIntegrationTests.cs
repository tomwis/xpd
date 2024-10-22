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
