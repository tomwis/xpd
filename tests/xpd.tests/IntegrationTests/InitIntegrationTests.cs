using System.IO.Abstractions;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FluentAssertions;
using NUnit.Framework;
using xpd.Models;
using xpd.Services;
using xpd.tests.Extensions;

namespace xpd.tests.IntegrationTests;

public class InitIntegrationTests : InitTestsBase
{
    [Test]
    public void WhenInitParseIsCalled_ThenDotnetToolsManifestIsCreatedAndToolsAreInstalled()
    {
        // Arrange
        const string solutionName = "solutionName";
        var outputPath = PrepareOutputDir();
        var init = GetSubject(
            solutionName,
            fileSystem: new FileSystem(),
            processProvider: new ProcessProvider(),
            outputDir: outputPath
        );

        // Act
        _ = init.Parse(init);

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
        var init = GetSubject(
            solutionName,
            fileSystem: new FileSystem(),
            processProvider: new ProcessProvider(),
            outputDir: outputPath
        );

        // Act
        _ = init.Parse(init);

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
        var init = GetSubject(
            solutionName,
            fileSystem: new FileSystem(),
            processProvider: new ProcessProvider(),
            outputDir: outputPath
        );

        // Act
        _ = init.Parse(init);

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
        var rootRepoFolder = GetRootRepoFolder();
        var outputPath = Path.Combine(rootRepoFolder, "..", outputDir);
        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }

        return outputPath;
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

    public class DotnetToolsManifest
    {
        [JsonPropertyName("tools")]
        public Dictionary<string, JsonObject> Tools { get; set; } = null!;
    }
}
