using System.Diagnostics;
using System.IO.Abstractions;
using System.Text.Json;
using System.Xml.Linq;
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

        CreateProjectAndSolution(solutionOutputDir, solutionName, projectName);
        CreateDirectoryBuildTargets(mainFolder);
        CreateDirectoryPackagesProps(mainFolder);
        InitializeGitRepository(mainFolder);
        InstallDotnetTools(mainFolder);
        var huskyHooksResult = InitializeHuskyHooks(mainFolder);
        if (huskyHooksResult is not null)
        {
            return huskyHooksResult;
        }

        return InitResult.Success(
            solutionName,
            projectName,
            mainFolder,
            solutionOutputDir,
            selectedFolders
        );
    }

    private void CreateFolders(string mainFolder, List<string> folders)
    {
        _fileSystem.Directory.CreateDirectory(mainFolder);
        folders.ForEach(folder =>
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(mainFolder, folder))
        );
    }

    private void CreateProjectAndSolution(
        string solutionOutputDir,
        string solutionName,
        string projectName
    )
    {
        RunCommand("dotnet", $"new sln --name \"{solutionName}\" --output \"{solutionOutputDir}\"");
        RunCommand("dotnet", $"new console --output \"{projectName}\"", solutionOutputDir);
        RunCommand("dotnet", $"sln add \"{projectName}\"", solutionOutputDir);
    }

    private void CreateDirectoryBuildTargets(string mainFolder)
    {
        var directoryBuildTargetsFile = _fileSystem.Path.Combine(
            mainFolder,
            "Directory.Build.targets"
        );
        var doc = new XDocument(new XElement("Project"));
        _fileSystem.File.WriteAllText(directoryBuildTargetsFile, doc.ToString());
    }

    private void CreateDirectoryPackagesProps(string mainFolder)
    {
        var directoryPackagesPropsFile = _fileSystem.Path.Combine(
            mainFolder,
            "Directory.Packages.props"
        );
        var doc = new XDocument(
            new XElement(
                "Project",
                new XElement(
                    "PropertyGroup",
                    new XElement("ManagePackageVersionsCentrally", "true")
                ),
                new XElement("ItemGroup", new XAttribute("Label", "App")),
                new XElement("ItemGroup", new XAttribute("Label", "Tests"))
            )
        );
        _fileSystem.File.WriteAllText(directoryPackagesPropsFile, doc.ToString());
    }

    private void InstallDotnetTools(string mainFolder)
    {
        RunCommand("dotnet", "new tool-manifest", mainFolder);
        RunCommand("dotnet", "tool install csharpier", mainFolder);
        RunCommand("dotnet", "tool install husky", mainFolder);
        RunCommand("dotnet", "husky install", mainFolder);
    }

    private InitResult? InitializeHuskyHooks(string mainFolder)
    {
        RunCommand(
            "dotnet",
            "husky add pre-commit -c \"dotnet husky run --group pre-commit\"",
            mainFolder
        );

        var taskRunnerPath = _fileSystem.Path.Combine(mainFolder, ".husky", "task-runner.json");
        if (!_fileSystem.File.Exists(taskRunnerPath))
        {
            Console.WriteLine(
                "Warning: .husky/task-runner.json doesn't exist. Git hooks were not added."
            );

            return InitResult.WithError(InitError.HuskyTaskRunnerMissing);
        }

        var taskRunnerJson = _fileSystem.File.ReadAllText(taskRunnerPath);
        var taskRunner = JsonSerializer.Deserialize<TaskRunner>(taskRunnerJson);

        if (taskRunner is null)
        {
            Console.WriteLine(
                "Warning: task-runner.json couldn't be parsed. Git hooks were not added."
            );

            return InitResult.WithError(InitError.HuskyTaskRunnerError);
        }

        taskRunner.Tasks.Clear();
        taskRunner.Tasks.Add(
            new TaskRunnerTask
            {
                Name = "format-staged-files-with-csharpier",
                Group = "pre-commit",
                Command = "dotnet",
                Arguments = ["csharpier", "${staged}"],
                Include = ["**/*.cs"],
            }
        );

        var options = new JsonSerializerOptions { WriteIndented = true };
        taskRunnerJson = JsonSerializer.Serialize(taskRunner, options);
        _fileSystem.File.WriteAllText(taskRunnerPath, taskRunnerJson);

        return null;
    }

    private void InitializeGitRepository(string mainFolder)
    {
        RunCommand("git", "init", mainFolder);
    }

    private void RunCommand(string command, string arguments, string workingDirectory = "")
    {
        ProcessStartInfo processInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory,
        };

        using var process = _processProvider.Start(processInfo);
        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        Console.WriteLine($"Output of command '{command} {arguments}': {result}");
    }
}
