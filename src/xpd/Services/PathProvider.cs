using System.IO.Abstractions;
using xpd.Constants;

namespace xpd.Services;

internal sealed class PathProvider(IFileSystem fileSystem, Init init, string solutionName)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly Init _init = init;
    private readonly string _solutionName = solutionName;

    internal IDirectoryInfo OutputDir =>
        AsDir(_init.Output ?? _fileSystem.Directory.GetCurrentDirectory());

    internal IDirectoryInfo MainFolder =>
        AsDir(_fileSystem.Path.Combine(OutputDir.FullName, _solutionName));

    internal IDirectoryInfo SrcDir =>
        AsDir(_fileSystem.Path.Combine(MainFolder.FullName, OptionalFoldersConstants.SrcDir));

    internal IDirectoryInfo BuildDir =>
        AsDir(_fileSystem.Path.Combine(MainFolder.FullName, OptionalFoldersConstants.BuildDir));

    internal IDirectoryInfo TestsDir =>
        AsDir(_fileSystem.Path.Combine(MainFolder.FullName, OptionalFoldersConstants.TestsDir));

    internal IFileInfo DirectoryPackagesPropsFile =>
        AsFile(_fileSystem.Path.Combine(MainFolder.FullName, FileConstants.DirectoryPackagesProps));

    internal IFileInfo DirectoryBuildTargetsFile =>
        AsFile(_fileSystem.Path.Combine(MainFolder.FullName, FileConstants.DirectoryBuildTargets));

    internal IFileInfo GetTestProjectFile(string testProjectName) =>
        AsFile(
            _fileSystem.Path.Combine(
                GetTestProjectDir(testProjectName).FullName,
                $"{testProjectName}.csproj"
            )
        );

    internal IFileInfo GetProjectFile(string projectName) =>
        AsFile(
            _fileSystem.Path.Combine(GetProjectDir(projectName).FullName, $"{projectName}.csproj")
        );

    internal IFileInfo HuskyTaskRunnerJson =>
        AsFile(
            _fileSystem.Path.Combine(MainFolder.FullName, ".husky", FileConstants.TaskRunnerJson)
        );

    internal IDirectoryInfo GetTestProjectDir(string testProjectName) =>
        AsDir(_fileSystem.Path.Combine(TestsDir.FullName, testProjectName));

    internal IDirectoryInfo GetProjectDir(string projectName) =>
        AsDir(_fileSystem.Path.Combine(SrcDir.FullName, projectName));

    internal IFileInfo DotnetToolsInstalledFile =>
        AsFile(
            _fileSystem.Path.Combine(
                MainFolder.FullName,
                OptionalFoldersConstants.ConfigDir,
                FileConstants.DotnetToolsInstalled
            )
        );

    public IFileInfo GitIgnoreFile =>
        AsFile(_fileSystem.Path.Combine(MainFolder.FullName, ".gitignore"));

    internal IFileInfo GetSolutionFile(string solutionName) =>
        AsFile(_fileSystem.Path.Combine(MainFolder.FullName, $"{solutionName}.sln"));

    internal IFileInfo EditorConfigFile =>
        AsFile(_fileSystem.Path.Combine(MainFolder.FullName, FileConstants.EditorConfig));

    public IDirectoryInfo GitHubWorkflowsDir =>
        AsDir(
            _fileSystem.Path.Combine(
                MainFolder.FullName,
                DirectoryConstants.GitHubDirName,
                DirectoryConstants.GitHubWorkflowsDirName
            )
        );

    internal IDirectoryInfo GetTestProjectIntegrationTestsDir(string testProjectName) =>
        AsDir(
            _fileSystem.Path.Combine(
                GetTestProjectDir(testProjectName).FullName,
                DirectoryConstants.IntegrationTestsDirName
            )
        );

    internal IFileInfo ReleaseNugetScriptFile =>
        AsFile(
            _fileSystem.Path.Combine(
                MainFolder.FullName,
                OptionalFoldersConstants.BuildDir,
                FileConstants.ReleaseNugetScript
            )
        );

    internal IFileInfo EnvFile =>
        AsFile(
            _fileSystem.Path.Combine(
                MainFolder.FullName,
                OptionalFoldersConstants.ConfigDir,
                FileConstants.Env
            )
        );

    internal IFileInfo EnvExampleFile =>
        AsFile(
            _fileSystem.Path.Combine(
                MainFolder.FullName,
                OptionalFoldersConstants.ConfigDir,
                FileConstants.EnvExample
            )
        );

    private IFileInfo AsFile(string path)
    {
        return _fileSystem.FileInfo.New(path);
    }

    private IDirectoryInfo AsDir(string path)
    {
        return _fileSystem.DirectoryInfo.New(path);
    }
}
