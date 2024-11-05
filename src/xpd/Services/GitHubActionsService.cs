using System.IO.Abstractions;
using xpd.Models;

namespace xpd.Services;

internal class GitHubActionsService(PathProvider pathProvider, IFileSystem fileSystem)
{
    private readonly PathProvider _pathProvider = pathProvider;
    private readonly IFileSystem _fileSystem = fileSystem;

    internal void AddBuildTestLintAction(ProjectType projectType)
    {
        const string buildTestLintFileName = "build-test-lint.yml";
        const string buildTestLintTemplateFileName = $"{buildTestLintFileName}.template";
        const string dotnetWorkloadInstallToken = "{DotnetWorkloadInstall}";
        const string runsOnToken = "{RunsOn}";
        var workflowsDir = _pathProvider.GitHubWorkflowsDir;

        CreateDirectoryIfNotExists(workflowsDir);

        var runsOn = GetRunsOn(projectType);
        var dotnetWorkloadInstall = GetDotnetWorkloadInstallStep(projectType);

        var content = ResourceProvider
            .GetResource(buildTestLintTemplateFileName)
            .Replace(runsOnToken, runsOn)
            .Replace(dotnetWorkloadInstallToken, dotnetWorkloadInstall);

        var filePath = _fileSystem.Path.Combine(workflowsDir.FullName, buildTestLintFileName);
        _fileSystem.File.WriteAllText(filePath, content);
    }

    private void CreateDirectoryIfNotExists(IDirectoryInfo workflowsDir)
    {
        if (!_fileSystem.Directory.Exists(workflowsDir.FullName))
        {
            _fileSystem.Directory.CreateDirectory(workflowsDir.FullName);
        }
    }

    private static string GetRunsOn(ProjectType projectType)
    {
        const string template = "runs-on: {0}";
        ProjectType[] projectsForMacos =
        [
            ProjectTypes.MauiApp,
            ProjectTypes.MauiLib,
            ProjectTypes.Ios,
            ProjectTypes.IosLib,
            ProjectTypes.IosBinding,
            ProjectTypes.IosTabbed,
        ];

        var os = projectsForMacos.Contains(projectType) ? "macos-latest" : "ubuntu-latest";
        return string.Format(template, os);
    }

    private static string GetDotnetWorkloadInstallStep(ProjectType projectType)
    {
        var map = new Dictionary<ProjectType, string[]>
        {
            [ProjectTypes.MauiApp] = ["maui"],
            [ProjectTypes.MauiLib] = ["maui"],
            [ProjectTypes.Ios] = ["ios"],
            [ProjectTypes.IosLib] = ["ios"],
            [ProjectTypes.IosBinding] = ["ios"],
            [ProjectTypes.IosTabbed] = ["ios"],
            [ProjectTypes.Android] = ["android"],
            [ProjectTypes.AndroidLib] = ["android"],
            [ProjectTypes.AndroidBindingLib] = ["android"],
            [ProjectTypes.ConsoleApp] = [],
        };

        var workloads = string.Join(" ", map[projectType]);

        if (string.IsNullOrEmpty(workloads))
            return string.Empty;

        const string stepTemplate = """
            - name: Install dotnet workloads
            run: {0}
            """;
        string dotnetWorkloadInstall = $"dotnet workload install {workloads}";
        return string.Format(stepTemplate, dotnetWorkloadInstall);
    }
}
