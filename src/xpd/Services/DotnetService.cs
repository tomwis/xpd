using System.IO.Abstractions;

namespace xpd.Services;

internal class DotnetService(CommandService commandService, PathProvider pathProvider)
{
    private readonly CommandService _commandService = commandService;
    private readonly PathProvider _pathProvider = pathProvider;

    public void CreateProjectAndSolution(
        IDirectoryInfo solutionOutputDir,
        string solutionName,
        IDirectoryInfo projectOutputDir,
        string projectName
    )
    {
        _commandService.RunCommand(
            "dotnet",
            $"new sln --name \"{solutionName}\" --output \"{solutionOutputDir.FullName}\""
        );
        _commandService.RunCommand(
            "dotnet",
            $"new console --output \"{projectName}\"",
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

        return testProjectName;
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
}
