using System.Diagnostics;
using System.IO.Abstractions;
using System.Reflection;
using CommandLine;
using Sharprompt;
using xpd.Exceptions;

namespace xpd;

[Verb("init")]
public class Init(IFileSystem fileSystem)
{
    private readonly IFileSystem _fileSystem = fileSystem;

    public Init()
        : this(new FileSystem()) { }

    [Option('o', "output", Required = false, HelpText = "Parent folder for solution.")]
    public string? Output { get; set; }

    public int Parse(Init args)
    {
        var solutionName = Prompt.Input<string>("Enter solution name");
        if (string.IsNullOrEmpty(solutionName))
        {
            Console.WriteLine("Solution name is required.");
            return 1;
        }

        var outputDir = args.Output ?? _fileSystem.Directory.GetCurrentDirectory();
        var solutionPath = _fileSystem.Path.Combine(outputDir, solutionName);
        var solutionDirectoryInfo = new DirectoryInfo(solutionPath);
        if (solutionDirectoryInfo.Exists)
        {
            Console.WriteLine($"Directory '{solutionName}' already exists in current directory.");
            return 1;
        }

        var projectName = Prompt.Input<string>("Enter project name", solutionName);
        if (string.IsNullOrEmpty(projectName))
        {
            Console.WriteLine("Project name will be the same as solution name.");
            projectName = solutionName;
        }

        var options = new[] { "src", "tests", "samples", "docs", "build", "config" };
        const int minimum = 0;
        var selectedFolders = Prompt
            .MultiSelect("Create folders", options, minimum: minimum, defaultValues: options)
            .ToList();

        CreateFolders(outputDir, solutionName, selectedFolders);

        string solutionOutputDir = selectedFolders.Contains("src")
            ? _fileSystem.Path.Combine(outputDir, solutionName, "src")
            : _fileSystem.Path.Combine(outputDir, solutionName);

        CreateProjectAndSolution(solutionOutputDir, solutionName, projectName);
        return 0;
    }

    private void CreateFolders(string outputDir, string solutionName, List<string> folders)
    {
        var mainFolder = _fileSystem.Path.Combine(outputDir, solutionName);
        _fileSystem.Directory.CreateDirectory(mainFolder);
        folders.ForEach(folder =>
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(mainFolder, folder))
        );
    }

    private static void CreateProjectAndSolution(
        string solutionOutputDir,
        string solutionName,
        string projectName
    )
    {
        RunCommand("dotnet", $"new sln --name \"{solutionName}\" --output \"{solutionOutputDir}\"");
        RunCommand("dotnet", $"new console --output \"{projectName}\"", solutionOutputDir);
        RunCommand("dotnet", $"sln add \"{projectName}\"", solutionOutputDir);
    }

    private static void RunCommand(string command, string arguments, string workingDirectory = "")
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

        using var process = Process.Start(processInfo);

        if (process is null)
        {
            throw new ProcessException(
                "Failed to start process. Id '{command}' installed and in PATH?"
            );
        }

        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        Console.WriteLine($"Output of command: {result}");
    }
}
