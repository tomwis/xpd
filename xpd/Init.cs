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

        CreateProjectAndSolution(outputDir, solutionName, projectName);
        return 0;
    }

    private static void CreateProjectAndSolution(
        string solutionOutputDir,
        string solutionName,
        string projectName
    )
    {
        RunCommand("dotnet", $"new sln --output \"{solutionName}\"", solutionOutputDir);
        var solutionDir = Path.Combine(solutionOutputDir, solutionName);
        RunCommand("dotnet", $"new console --output \"{projectName}\"", solutionDir);
        RunCommand("dotnet", $"sln add \"{projectName}\"", solutionDir);
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
