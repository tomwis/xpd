using System.IO.Abstractions;
using System.Xml.Linq;
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
        var testProjectPath = _pathProvider.GetTestProjectDir(testProjectName);
        _commandService.RunCommand(
            "dotnet",
            $"new nunit --name {testProjectName}",
            testProjectPath.Parent!.FullName
        );
        _commandService.RunCommand(
            "dotnet",
            $"sln add \"{testProjectPath.FullName}\" --solution-folder Tests",
            solutionOutputDir.FullName
        );

        AddDefaultFoldersToTestProject(testProjectName);

        return testProjectName;
    }

    private void AddDefaultFoldersToTestProject(string testProjectName)
    {
        var testProjectDir = _pathProvider.GetTestProjectDir(testProjectName);
        CreateFolder("UnitTests");
        CreateFolder("IntegrationTests");

        var testProjectFile = _pathProvider.GetTestProjectFile(testProjectName);
        var testProjectContent = _fileSystem.File.ReadAllText(testProjectFile.FullName);
        var xml = XDocument.Parse(testProjectContent);
        xml.Root!.Add(
            new XElement("ItemGroup", AddFolder("UnitTests"), AddFolder("IntegrationTests"))
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

    public void AddNugetsToTestProject(IFileInfo testProjectFile, params string[] nugetPackages)
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
