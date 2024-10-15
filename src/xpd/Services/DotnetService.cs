using System.IO.Abstractions;

namespace xpd.Services;

public class DotnetService(CommandService commandService, IFileSystem fileSystem)
{
    private readonly CommandService _commandService = commandService;
    private readonly IFileSystem _fileSystem = fileSystem;

    public void CreateProjectAndSolution(
        string solutionOutputDir,
        string solutionName,
        string projectName
    )
    {
        _commandService.RunCommand(
            "dotnet",
            $"new sln --name \"{solutionName}\" --output \"{solutionOutputDir}\""
        );
        _commandService.RunCommand(
            "dotnet",
            $"new console --output \"{projectName}\"",
            solutionOutputDir
        );
        _commandService.RunCommand("dotnet", $"sln add \"{projectName}\"", solutionOutputDir);
    }

    public (string testProjectName, string testProjectPath) CreateTestProject(
        string solutionOutputDir,
        string testsOutputDir,
        string projectName
    )
    {
        var testProjectName = $"{projectName}.Tests";
        var testProjectPath = _fileSystem.Path.Combine(testsOutputDir, testProjectName);
        testProjectPath = _fileSystem.Path.GetFullPath(testProjectPath);
        _commandService.RunCommand("dotnet", $"new nunit --name {testProjectName}", testsOutputDir);
        _commandService.RunCommand(
            "dotnet",
            $"sln add \"{testProjectPath}\" --solution-folder Tests",
            solutionOutputDir
        );

        return (testProjectName, testProjectPath);
    }

    public void InstallDotnetTools(string mainFolder)
    {
        _commandService.RunCommand("dotnet", "new tool-manifest", mainFolder);
        _commandService.RunCommand("dotnet", "tool install csharpier", mainFolder);
        _commandService.RunCommand("dotnet", "tool install husky", mainFolder);
        _commandService.RunCommand("dotnet", "husky install", mainFolder);
    }
}
