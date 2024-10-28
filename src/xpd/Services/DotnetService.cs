using System.IO.Abstractions;
using System.Xml.Linq;
using xpd.Constants;
using xpd.Models;

namespace xpd.Services;

internal class DotnetService(
    CommandService commandService,
    PathProvider pathProvider,
    IFileSystem fileSystem
)
{
    private readonly CommandService _commandService = commandService;
    private readonly PathProvider _pathProvider = pathProvider;
    private readonly IFileSystem _fileSystem = fileSystem;

    public void CreateProjectAndSolution(
        IDirectoryInfo solutionOutputDir,
        string solutionName,
        IDirectoryInfo projectOutputDir,
        string projectName,
        ProjectType projectType
    )
    {
        _commandService.RunCommand(
            "dotnet",
            $"new sln --name \"{solutionName}\" --output \"{solutionOutputDir.FullName}\""
        );
        _commandService.RunCommand(
            "dotnet",
            $"new {projectType} --output \"{projectName}\"",
            projectOutputDir.FullName
        );

        var projectPath = _pathProvider.GetProjectDir(projectName);
        _commandService.RunCommand(
            "dotnet",
            $"sln add \"{projectPath.FullName}\" --in-root",
            solutionOutputDir.FullName
        );
    }

    public string CreateTestProject(IDirectoryInfo solutionOutputDir, string projectName)
    {
        var testProjectName = $"{projectName}.Tests";
        var testProjectDir = _pathProvider.GetTestProjectDir(testProjectName);
        CreateTestProjectAndAddToSolution(solutionOutputDir, testProjectName, testProjectDir);
        AddDefaultFoldersToTestProject(testProjectName);
        AddSetupFixtureWithGitHookCheckForIntegrationTests(testProjectName);
        AddNugetPackagesToTestProject(
            _pathProvider.GetTestProjectFile(testProjectName),
            "FluentAssertions",
            "FluentAssertions.Analyzers",
            "NSubstitute",
            "NSubstitute.Analyzers.CSharp",
            "AutoFixture",
            "AutoFixture.AutoNSubstitute"
        );
        return testProjectName;
    }

    public string CreateConventionTestProject(IDirectoryInfo solutionOutputDir, string projectName)
    {
        var testProjectName = $"{projectName}.ConventionTests";
        var testProjectDir = _pathProvider.GetTestProjectDir(testProjectName);
        CreateTestProjectAndAddToSolution(solutionOutputDir, testProjectName, testProjectDir);
        AddNugetPackagesToTestProject(
            _pathProvider.GetTestProjectFile(testProjectName),
            "FluentAssertions",
            "FluentAssertions.Analyzers"
        );
        return testProjectName;
    }

    private void CreateTestProjectAndAddToSolution(
        IDirectoryInfo solutionOutputDir,
        string testProjectName,
        IDirectoryInfo testProjectDir
    )
    {
        _commandService.RunCommand(
            "dotnet",
            $"new nunit --name {testProjectName}",
            testProjectDir.Parent!.FullName
        );
        _commandService.RunCommand(
            "dotnet",
            $"sln add \"{testProjectDir.FullName}\" --solution-folder Tests",
            solutionOutputDir.FullName
        );
    }

    private void AddDefaultFoldersToTestProject(string testProjectName)
    {
        var testProjectDir = _pathProvider.GetTestProjectDir(testProjectName);
        CreateFolder(DirectoryConstants.UnitTestsDirName);
        CreateFolder(DirectoryConstants.IntegrationTestsDirName);

        var testProjectFile = _pathProvider.GetTestProjectFile(testProjectName);
        var testProjectContent = _fileSystem.File.ReadAllText(testProjectFile.FullName);
        var xml = XDocument.Parse(testProjectContent);
        xml.Root!.Add(
            new XElement(
                "ItemGroup",
                AddFolder(DirectoryConstants.UnitTestsDirName),
                AddFolder(DirectoryConstants.IntegrationTestsDirName)
            )
        );
        _fileSystem.File.WriteAllText(testProjectFile.FullName, xml.ToString());
        return;

        void CreateFolder(string name) =>
            _fileSystem.Directory.CreateDirectory(
                _fileSystem.Path.Combine(testProjectDir.FullName, name)
            );

        static XElement AddFolder(string name) =>
            new("Folder", new XAttribute("Include", $"{name}\\"));
    }

    private void AddNugetPackagesToTestProject(
        IFileInfo testProjectFile,
        params string[] nugetPackages
    )
    {
        foreach (var package in nugetPackages)
        {
            _commandService.RunCommand(
                "dotnet",
                $"add package {package}",
                testProjectFile.Directory!.FullName
            );
        }
    }

    private void AddSetupFixtureWithGitHookCheckForIntegrationTests(string testProjectName)
    {
        const string setupFixtureFileName = "SetupFixture.cs";
        const string setupFixtureTemplateFileName = "SetupFixture.template";
        var integrationTestsDir = _pathProvider.GetTestProjectIntegrationTestsDir(testProjectName);
        var content = ResourceProvider
            .GetResource(setupFixtureTemplateFileName)
            .Replace("{ProjectName}", testProjectName);
        var setupFixtureFilePath = _fileSystem.Path.Combine(
            integrationTestsDir.FullName,
            setupFixtureFileName
        );
        _fileSystem.File.WriteAllText(setupFixtureFilePath, content);
    }

    public void InstallDotnetTools(IDirectoryInfo mainFolder)
    {
        _commandService.RunCommand("dotnet", "new tool-manifest", mainFolder.FullName);
        _commandService.RunCommand("dotnet", "tool install csharpier", mainFolder.FullName);
        _commandService.RunCommand("dotnet", "tool install husky", mainFolder.FullName);
        _commandService.RunCommand("dotnet", "husky install", mainFolder.FullName);
    }

    public void CreateGitIgnore(IDirectoryInfo mainFolder)
    {
        _commandService.RunCommand("dotnet", "new gitignore", mainFolder.FullName);
    }
}
