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

    internal IFileInfo GetSolutionFile(string solutionName) =>
        AsFile(_fileSystem.Path.Combine(MainFolder.FullName, $"{solutionName}.sln"));

    private IFileInfo AsFile(string path)
    {
        return _fileSystem.FileInfo.New(path);
    }

    private IDirectoryInfo AsDir(string path)
    {
        return _fileSystem.DirectoryInfo.New(path);
    }
}
