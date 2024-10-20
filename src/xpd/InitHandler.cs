using System.IO.Abstractions;
using xpd.Constants;
using xpd.Enums;
using xpd.Interfaces;
using xpd.Models;
using xpd.Services;

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
    private MsBuildService _msBuildService;
    private PathProvider _pathProvider;

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

        var dotnetService = new DotnetService(_commandService, _pathProvider);
        var solutionOutputDir = mainFolder;
        var projectOutputDir = _pathProvider.SrcDir;
        dotnetService.CreateProjectAndSolution(
            solutionOutputDir,
            solutionName,
            projectOutputDir,
            projectName
        );

        string testProjectName = dotnetService.CreateTestProject(solutionOutputDir, projectName);
        var nugetPackagesForTestProject = new[]
        {
            "FluentAssertions",
            "NSubstitute",
            "NSubstitute.Analyzers.CSharp",
            "AutoFixture",
            "AutoFixture.AutoNSubstitute",
        };
        dotnetService.AddNugetsToTestProject(
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
        dotnetService.InstallDotnetTools(mainFolder);

        var huskyService = new HuskyService(_fileSystem, _commandService, _pathProvider);
        var huskyHooksResult = huskyService.InitializeHuskyHooks(mainFolder);
        if (huskyHooksResult is not null)
        {
            return huskyHooksResult;
        }

        huskyService.InitializeHuskyRestoreTarget();

        return InitResult.Success(
            solutionName,
            projectName,
            mainFolder.FullName,
            foldersToCreate,
            _pathProvider.GetTestProjectDir(testProjectName).FullName
        );
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
}
