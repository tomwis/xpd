using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using xpd.Interfaces;
using xpd.Models;
using xpd.Services;
using xpd.Tests.Assertions.Extensions;
using xpd.Tests.Assertions.Models;
using xpd.Tests.Extensions;
using PathProvider = xpd.Tests.Utilities.PathProvider;

namespace xpd.Tests.IntegrationTests;

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
        var tasksJson = JsonSerializer.Serialize(
            tasks,
            new JsonSerializerOptions { WriteIndented = true }
        );
        var errorMessage = $"actual tasks list is as following: {tasksJson}";
        tasks.Should().ContainSingle(task => IsCsharpierTask(task), errorMessage);
        tasks.Should().ContainSingle(task => IsBuildTask(task), errorMessage);
        tasks.Should().ContainSingle(task => IsUnitTestsTask(task), errorMessage);
        tasks.Should().ContainSingle(task => IsConventionTestsTask(task), errorMessage);
    }

    [Test]
    public void PreCommitGitHookIsRunningHuskyPreCommitGroup()
    {
        // Assert
        var preCommitHook = Path.Combine(_outputPath, SolutionName, ".husky", "pre-commit")
            .ToFile()
            .ReadAllLines();
        preCommitHook
            .Should()
            .ContainSingle(line => line.StartsWith("dotnet husky run --group pre-commit"));
    }

    [Test]
    public void PreCommitGitHookHasFlagSet()
    {
        // Assert
        var preCommitHook = Path.Combine(_outputPath, SolutionName, ".husky", "pre-commit")
            .ToFile()
            .ReadAllLines();
        preCommitHook
            .Should()
            .ContainSingle(line => line.StartsWith("export GIT_HOOK_EXECUTION=true"));
    }

    [Test]
    public void SolutionIsBuildingSuccessfully()
    {
        // Assert
        var slnDir = Path.Combine(_outputPath, SolutionName);
        var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = slnDir,
            }
        );
        process!.WaitForExit();
        process.StandardOutput.ReadToEnd().Should().NotBeEmpty();
        process.StandardError.ReadToEnd().Should().BeEmpty();
    }

    private static bool IsCsharpierTask(TaskRunnerTask task) =>
        IsPreCommitTask(task)
        && IsDotnetCommand(task)
        && task.Arguments.First().Equals("csharpier");

    private static bool IsBuildTask(TaskRunnerTask task) =>
        IsPreCommitTask(task) && IsDotnetCommand(task) && task.Arguments.First().Equals("build");

    private static bool IsUnitTestsTask(TaskRunnerTask task) =>
        IsPreCommitTask(task)
        && IsDotnetCommand(task)
        && task.Arguments.SequenceEqual(
            ["test", "--filter", "FullyQualifiedName~.Tests.UnitTests", "--no-build"]
        );

    [SuppressMessage("Performance", "SYSLIB1045:Konwertuj na atrybut „GeneratedRegexAttribute”.")]
    private static bool IsConventionTestsTask(TaskRunnerTask task) =>
        IsPreCommitTask(task)
        && IsDotnetCommand(task)
        && task.Arguments.First().Equals("test")
        && Regex.IsMatch(task.Arguments.Skip(1).First(), "tests/.*\\.ConventionTests")
        && task.Arguments.Last().Equals("--no-build");

    private static bool IsDotnetCommand(TaskRunnerTask task) => task.Command.Equals("dotnet");

    private static bool IsPreCommitTask(TaskRunnerTask task) => task.Group.Equals("pre-commit");

    [Test]
    public void TestProjectHasAllNugetPackagesInstalled()
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
                "FluentAssertions.Analyzers",
                "NSubstitute",
                "NSubstitute.Analyzers.CSharp",
                "AutoFixture",
                "AutoFixture.AutoNSubstitute"
            );
    }

    [Test]
    public void ConventionTestProjectHasAllNugetPackagesInstalled()
    {
        // Arrange
        const string testProjectName = $"{SolutionName}.ConventionTests";
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
            .Contain("FluentAssertions", "FluentAssertions.Analyzers");
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

    [TestCase("UnitTests\\")]
    [TestCase("IntegrationTests\\")]
    public void DefaultFoldersAreAddedToTestsCsproj(string expectedFolder)
    {
        // Arrange
        var path = Path.Combine(
            _outputPath,
            SolutionName,
            "tests",
            $"{SolutionName}.Tests",
            $"{SolutionName}.Tests.csproj"
        );

        var xml = XDocument.Load(path);

        // Assert
        xml.Should().HaveElementWithFolder(expectedFolder);
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
