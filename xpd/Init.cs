using System.Diagnostics;
using System.Reflection;
using CommandLine;
using Sharprompt;

namespace xpd;

[Verb("init")]
public class Init
{
    [Option('o', "output", Required = false, HelpText = "Parent folder for solution.")]
    public string? Output { get; set; }

    public static int Parse(Init args)
    {
        var solutionName = Prompt.Input<string>("Enter solution name");
        if (string.IsNullOrEmpty(solutionName))
        {
            Console.WriteLine("Solution name is required.");
            return 1;
        }

        var outputDir = args.Output ?? Directory.GetCurrentDirectory();
        var solutionPath = Path.Combine(outputDir, solutionName);
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
            ? Path.Combine(outputDir, solutionName, "src")
            : Path.Combine(outputDir, solutionName);

        CreateProjectAndSolution(solutionOutputDir, solutionName, projectName);
        return 0;
    }

    private static void CreateFolders(string outputDir, string solutionName, List<string> folders)
    {
        var mainFolder = Path.Combine(outputDir, solutionName);
        Directory.CreateDirectory(mainFolder);
        folders.ForEach(folder => Directory.CreateDirectory(Path.Combine(mainFolder, folder)));
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

        using Process? process = Process.Start(processInfo);
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        Console.WriteLine("Output of command: " + result);
    }
}
