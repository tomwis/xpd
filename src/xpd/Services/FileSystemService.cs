using System.IO.Abstractions;

namespace xpd.Services;

internal class FileSystemService(
    IFileSystem fileSystem,
    DotnetService dotnetService,
    PathProvider pathProvider
)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly DotnetService _dotnetService = dotnetService;
    private readonly PathProvider _pathProvider = pathProvider;

    internal void CreateFolders(IDirectoryInfo mainFolder, List<string> folders)
    {
        mainFolder.Create();
        folders.ForEach(folder => mainFolder.CreateSubdirectory(folder));
    }

    internal void AddGitIgnore(IDirectoryInfo mainFolder)
    {
        _dotnetService!.CreateGitIgnore(mainFolder);
        string[] gitIgnoreAdditionalContent =
        [
            Environment.NewLine,
            "# Added by xpd init",
            "/config/dotnet_tools_installed.txt",
        ];

        _fileSystem.File.AppendAllLines(
            _pathProvider!.GitIgnoreFile.FullName,
            gitIgnoreAdditionalContent
        );
    }

    internal void AddReadme(
        IDirectoryInfo mainFolder,
        string solutionName,
        string projectName,
        string testProjectName,
        string conventionTestProjectName
    )
    {
        const string readmeName = "README.md";
        var content = ResourceProvider.GetResource(readmeName);
        content = content
            .Replace("{solutionName}", solutionName)
            .Replace("{projectName}", projectName)
            .Replace("{testProjectName}", testProjectName)
            .Replace("{conventionTestProjectName}", conventionTestProjectName);

        var readmePath = _fileSystem.Path.Combine(mainFolder.FullName, "README.md");
        _fileSystem.File.WriteAllText(readmePath, content);
    }

    internal void AddEditorConfig(IFileInfo editorConfigFile)
    {
        var content = ResourceProvider.GetResource(editorConfigFile.Name);
        _fileSystem.File.WriteAllText(editorConfigFile.FullName, content);
    }
}
