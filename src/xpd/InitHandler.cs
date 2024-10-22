using System.IO.Abstractions;
using xpd.Constants;
using xpd.Enums;
using xpd.Interfaces;
using xpd.Models;
using xpd.Services;
using xpd.SolutionModifier;

namespace xpd;

public class InitHandler(
    IFileSystem fileSystem,
    IInputRequester inputRequester,
    IProcessProvider processProvider
)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly IInputRequester _inputRequester = inputRequester;
    private readonly CommandService _commandService = new(processProvider);
    private MsBuildService? _msBuildService;
    private PathProvider? _pathProvider;
    private DotnetService? _dotnetService;

    public InitHandler()
        : this(new FileSystem(), new InputRequester(), new ProcessProvider()) { }

    public InitResult Parse(Init args)
    {
        var solutionName = args.SolutionName;
        if (string.IsNullOrEmpty(solutionName))
        {
            solutionName = _inputRequester.GetSolutionName();
        }

        if (string.IsNullOrEmpty(solutionName))
        {
            Console.WriteLine("Solution name is required.");
            return InitResult.WithError(InitError.SolutionNameRequired);
        }

        _pathProvider = new PathProvider(_fileSystem, args, solutionName);
        if (_pathProvider.MainFolder.Exists)
        {
            Console.WriteLine(
                $"Directory '{solutionName}' already exists in current directory ({_pathProvider.OutputDir.FullName})."
            );
            return InitResult.WithError(InitError.SolutionNameExists);
        }

        var projectName = solutionName;
        var foldersToCreate = new List<string>
        {
            OptionalFoldersConstants.SrcDir,
            OptionalFoldersConstants.TestsDir,
            OptionalFoldersConstants.SamplesDir,
            OptionalFoldersConstants.DocsDir,
            OptionalFoldersConstants.BuildDir,
            OptionalFoldersConstants.ConfigDir,
        };
        var mainFolder = _pathProvider.MainFolder;
        CreateFolders(mainFolder, foldersToCreate);

        _dotnetService = new DotnetService(_commandService, _pathProvider);
        var solutionOutputDir = mainFolder;
        var projectOutputDir = _pathProvider.SrcDir;
        _dotnetService.CreateProjectAndSolution(
            solutionOutputDir,
            solutionName,
            projectOutputDir,
            projectName
        );

        string testProjectName = _dotnetService.CreateTestProject(solutionOutputDir, projectName);
        var nugetPackagesForTestProject = new[]
        {
            "FluentAssertions",
            "NSubstitute",
            "NSubstitute.Analyzers.CSharp",
            "AutoFixture",
            "AutoFixture.AutoNSubstitute",
        };
        _dotnetService.AddNugetsToTestProject(
            _pathProvider.GetTestProjectFile(testProjectName),
            nugetPackagesForTestProject
        );

        _msBuildService = new(_fileSystem, _pathProvider);
        _msBuildService.CreateDirectoryBuildTargets();
        _msBuildService.CreateDirectoryPackagesProps();
        _msBuildService.MovePackageVersionsToDirectoryPackagesProps(
            _pathProvider.GetTestProjectFile(testProjectName)
        );
        InitializeGitRepository(mainFolder);
        CreateGitIgnore(mainFolder);
        _dotnetService.InstallDotnetTools(mainFolder);

        var huskyService = new HuskyService(_fileSystem, _commandService, _pathProvider);
        var huskyHooksResult = huskyService.InitializeHuskyHooks(mainFolder);
        if (huskyHooksResult is not null)
        {
            return huskyHooksResult;
        }

        huskyService.InitializeHuskyRestoreTarget();

        AddReadme(mainFolder, solutionName, projectName, testProjectName);
        AddEditorConfig(_pathProvider.EditorConfigFile);
        AddSolutionSettingsFolderWithItems(solutionName);

        return InitResult.Success(
            solutionName,
            projectName,
            mainFolder.FullName,
            foldersToCreate,
            _pathProvider.GetTestProjectDir(testProjectName).FullName
        );
    }

    private void AddEditorConfig(IFileInfo editorConfigFile)
    {
        var content = GetResource(editorConfigFile.Name);
        _fileSystem.File.WriteAllText(editorConfigFile.FullName, content);
    }

    private void CreateGitIgnore(IDirectoryInfo mainFolder)
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

    private void AddReadme(
        IDirectoryInfo mainFolder,
        string solutionName,
        string projectName,
        string testProjectName
    )
    {
        const string readmeName = "README.md";
        var content = GetResource(readmeName);
        content = content
            .Replace("{solutionName}", solutionName)
            .Replace("{projectName}", projectName)
            .Replace("{testProjectName}", testProjectName);

        var readmePath = _fileSystem.Path.Combine(mainFolder.FullName, "README.md");
        _fileSystem.File.WriteAllText(readmePath, content);
    }

    private static string GetResource(string readmeName)
    {
        var assembly = typeof(InitHandler).Assembly;
        var readmeResourceName = assembly
            .GetManifestResourceNames()
            .First(name => name.EndsWith(readmeName));
        using var stream = assembly.GetManifestResourceStream(readmeResourceName)!;
        var content = new StreamReader(stream).ReadToEnd();
        return content;
    }

    private void CreateFolders(IDirectoryInfo mainFolder, List<string> folders)
    {
        mainFolder.Create();
        folders.ForEach(folder => mainFolder.CreateSubdirectory(folder));
    }

    private void InitializeGitRepository(IDirectoryInfo mainFolder)
    {
        _commandService.RunCommand("git", "init", mainFolder.FullName);
    }

    private void AddSolutionSettingsFolderWithItems(string solutionName)
    {
        var solutionItems = new Dictionary<string, string>
        {
            { FileConstants.EditorConfig, FileConstants.EditorConfig },
            { FileConstants.GitIgnore, FileConstants.GitIgnore },
            { FileConstants.DirectoryBuildTargets, FileConstants.DirectoryBuildTargets },
            { FileConstants.DirectoryPackagesProps, FileConstants.DirectoryPackagesProps },
            {
                FileConstants.TaskRunnerJson,
                _fileSystem.Path.GetRelativePath(
                    _pathProvider!.MainFolder.FullName,
                    _pathProvider.HuskyTaskRunnerJson.FullName
                )
            },
        };

        var solutionFolder = new SolutionFolder("SolutionSettings");
        foreach (var (name, value) in solutionItems)
        {
            solutionFolder.AddItem(new SolutionItem(name, value));
        }

        var solutionFileInfo = _pathProvider.GetSolutionFile(solutionName);
        var slnContent = _fileSystem.File.ReadAllText(solutionFileInfo.FullName);
        var solutionFile = new SolutionFile(slnContent);
        solutionFile.AddSolutionFolder(solutionFolder);
        _fileSystem.File.WriteAllText(solutionFileInfo.FullName, solutionFile.ToString());
    }
}
