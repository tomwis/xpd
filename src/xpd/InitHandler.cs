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
        if (!TryGetSolutionName(args, out var slnName, out var initError))
        {
            return initError!;
        }

        var solutionName = slnName!;
        var selectedProjectType = args.ProjectType ?? ProjectTypes.ConsoleApp;

        _pathProvider = new PathProvider(_fileSystem, args, solutionName);
        _dotnetService = new DotnetService(_commandService, _pathProvider, _fileSystem);
        var fileSystemService = new FileSystemService(_fileSystem, _dotnetService, _pathProvider);
        var gitHubActionsService = new GitHubActionsService(_pathProvider, _fileSystem);

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
        fileSystemService.CreateFolders(mainFolder, foldersToCreate);

        var solutionOutputDir = mainFolder;
        var projectOutputDir = _pathProvider.SrcDir;
        _dotnetService.CreateProjectAndSolution(
            solutionOutputDir,
            solutionName,
            projectOutputDir,
            projectName,
            selectedProjectType
        );

        string testProjectName = _dotnetService.CreateTestProject(solutionOutputDir, projectName);
        var conventionTestProjectName = _dotnetService.CreateConventionTestProject(
            solutionOutputDir,
            projectName
        );

        _msBuildService = new(_fileSystem, _pathProvider);
        _msBuildService.CreateDirectoryBuildTargets();
        _msBuildService.CreateDirectoryPackagesProps();
        _msBuildService.MovePackageVersionsToDirectoryPackagesProps(
            _pathProvider.GetTestProjectFile(testProjectName)
        );

        _msBuildService.MovePackageVersionsToDirectoryPackagesProps(
            _pathProvider.GetTestProjectFile(conventionTestProjectName)
        );

        InitializeGitRepository(mainFolder);
        fileSystemService.AddGitIgnore(mainFolder);
        _dotnetService.InstallDotnetTools(mainFolder);

        var huskyService = new HuskyService(_fileSystem, _commandService, _pathProvider);
        var huskyHooksResult = huskyService.InitializeHuskyHooks(mainFolder, projectName);
        if (huskyHooksResult is not null)
        {
            return huskyHooksResult;
        }

        huskyService.InitializeHuskyRestoreTarget();

        fileSystemService.AddReadme(
            mainFolder,
            solutionName,
            projectName,
            testProjectName,
            conventionTestProjectName
        );
        fileSystemService.AddEditorConfig(_pathProvider.EditorConfigFile);
        AddSolutionSettingsFolderWithItems(solutionName);
        gitHubActionsService.AddBuildTestLintAction(selectedProjectType);
        fileSystemService.AddReleaseNugetScript(projectName);
        gitHubActionsService.AddGitHubReleaseAction(projectName);

        return InitResult.Success(
            solutionName,
            projectName,
            mainFolder.FullName,
            foldersToCreate,
            _pathProvider.GetTestProjectDir(testProjectName).FullName,
            _pathProvider.GetTestProjectDir(conventionTestProjectName).FullName
        );
    }

    private bool TryGetSolutionName(Init args, out string? solutionName, out InitResult? initError)
    {
        initError = null;
        solutionName = args.SolutionName;
        if (string.IsNullOrEmpty(solutionName))
        {
            solutionName = _inputRequester.GetSolutionName();
        }

        if (string.IsNullOrEmpty(solutionName))
        {
            Console.WriteLine("Solution name is required.");
            initError = InitResult.WithError(InitError.SolutionNameRequired);
            return false;
        }

        return true;
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
