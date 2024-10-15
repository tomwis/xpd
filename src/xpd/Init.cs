using System.IO.Abstractions;
using CommandLine;
using xpd.Enums;
using xpd.Interfaces;
using xpd.Models;
using xpd.Services;

namespace xpd;

[Verb("init")]
public class Init(
    IFileSystem fileSystem,
    IInputRequestor inputRequestor,
    IProcessProvider processProvider
)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly IInputRequestor _inputRequestor = inputRequestor;
    private readonly IProcessProvider _processProvider = processProvider;
    private readonly CommandService _commandService = new(processProvider);
    private readonly MsBuildService _msBuildService = new(fileSystem);

    public Init()
        : this(new FileSystem(), new InputRequestor(), new ProcessProvider()) { }

    [Option('o', "output", Required = false, HelpText = "Parent folder for solution.")]
    public string? Output { get; set; }

    public InitResult Parse(Init args)
    {
        var solutionName = _inputRequestor.GetSolutionName();
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

        var projectName = _inputRequestor.GetProjectName(solutionName);
        if (string.IsNullOrEmpty(projectName))
        {
            Console.WriteLine("Project name will be the same as solution name.");
            projectName = solutionName;
        }

        var selectedFolders = _inputRequestor.GetFoldersToCreate();
        var mainFolder = _fileSystem.Path.Combine(outputDir, solutionName);
        CreateFolders(mainFolder, selectedFolders);

        string solutionOutputDir = selectedFolders.Contains("src")
            ? _fileSystem.Path.Combine(outputDir, solutionName, "src")
            : mainFolder;

        string testsDir = selectedFolders.Contains("tests")
            ? _fileSystem.Path.Combine(mainFolder, "tests")
            : mainFolder;

        var dotnetService = new DotnetService(_commandService, _fileSystem);
        dotnetService.CreateProjectAndSolution(solutionOutputDir, solutionName, projectName);
        (string testProjectName, string testProjectPath) = dotnetService.CreateTestProject(
            solutionOutputDir,
            testsDir,
            projectName
        );

        _msBuildService.CreateDirectoryBuildTargets(mainFolder);
        _msBuildService.CreateDirectoryPackagesProps(mainFolder);
        const string directoryPackagesProps = "Directory.Packages.props";
        string directoryPackagePropsFilePath = _fileSystem.Path.Combine(
            mainFolder,
            directoryPackagesProps
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
            selectedFolders,
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
