using System.Diagnostics;
using xpd.Exceptions;
using xpd.Interfaces;

namespace xpd.Services;

public class CommandService(IProcessProvider processProvider)
{
    private readonly IProcessProvider _processProvider = processProvider;

    public void RunCommand(string command, string arguments, string workingDirectory = "")
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory,
            EnvironmentVariables = { { "DOTNET_CLI_UI_LANGUAGE", "en" } },
        };

        using var process = _processProvider.Start(processInfo);
        var stdOut = process.StandardOutput.ReadToEnd();
        var stdErr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (!string.IsNullOrEmpty(stdOut))
        {
            Console.WriteLine($"Stdout of command '{command} {arguments}': {stdOut}");
        }

        if (!string.IsNullOrEmpty(stdErr))
        {
            var message = $"Stderr of command '{command} {arguments}': {stdErr}";
            Console.WriteLine(message);
        }

        if (process.ExitCode != 0)
        {
            throw new CommandException(
                $"Command {command} {arguments} exited with code {process.ExitCode}"
            );
        }
    }
}
