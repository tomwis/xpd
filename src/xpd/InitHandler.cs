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
    private readonly MsBuildService _msBuildService = new(fileSystem);

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

        var outputDir = args.Output ?? _fileSystem.Directory.GetCurrentDirectory();
        var solutionPath = _fileSystem.Path.Combine(outputDir, solutionName);
        var solutionDirectoryInfo = _fileSystem.DirectoryInfo.New(solutionPath);
        if (solutionDirectoryInfo.Exists)
        {
            Console.WriteLine(
                $"Directory '{solutionName}' already exists in current directory ({outputDir})."
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
        var mainFolder = _fileSystem.Path.Combine(outputDir, solutionName);
        CreateFolders(mainFolder, foldersToCreate);

        var dotnetService = new DotnetService(_commandService, _fileSystem);
        string solutionOutputDir = mainFolder;
        string projectOutputDir = _fileSystem.Path.Combine(
            mainFolder,
            OptionalFoldersConstants.SrcDir
        );
        dotnetService.CreateProjectAndSolution(
            solutionOutputDir,
            solutionName,
            projectOutputDir,
            projectName
        );

        string testsDir = _fileSystem.Path.Combine(mainFolder, OptionalFoldersConstants.TestsDir);
        (string testProjectName, string testProjectPath) = dotnetService.CreateTestProject(
            solutionOutputDir,
            testsDir,
            projectName
        );

        _msBuildService.CreateDirectoryBuildTargets(mainFolder);
        _msBuildService.CreateDirectoryPackagesProps(mainFolder);
        string directoryPackagePropsFilePath = _fileSystem.Path.Combine(
            mainFolder,
            FileConstants.DirectoryPackagesProps
        );
        var testProjectFilePath = _fileSystem.Path.Combine(
            testProjectPath,
            $"{testProjectName}.csproj"
        );
        _msBuildService.MovePackageVersionsToDirectoryPackagesProps(
            testProjectFilePath,
            directoryPackagePropsFilePath
        );
        InitializeGitRepository(mainFolder);
        dotnetService.InstallDotnetTools(mainFolder);

        var huskyService = new HuskyService(_fileSystem, _commandService);
        var huskyHooksResult = huskyService.InitializeHuskyHooks(mainFolder);
        if (huskyHooksResult is not null)
        {
            return huskyHooksResult;
        }

        huskyService.InitializeHuskyRestoreTarget(mainFolder);

        return InitResult.Success(
            solutionName,
            projectName,
            mainFolder,
            foldersToCreate,
            solutionOutputDir,
            testProjectPath
        );
    }

    private void CreateFolders(string mainFolder, List<string> folders)
    {
        _fileSystem.Directory.CreateDirectory(mainFolder);
        folders.ForEach(folder =>
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(mainFolder, folder))
        );
    }

    private void InitializeGitRepository(string mainFolder)
    {
        _commandService.RunCommand("git", "init", mainFolder);
    }
}
