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
        var result = process.StandardOutput.ReadToEnd();
        var errors = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (!string.IsNullOrEmpty(errors))
        {
            var message = $"Command '{command} {arguments}' failed with errors: {errors}";
            Console.WriteLine(message);
            throw new CommandException(message);
        }

        Console.WriteLine($"Output of command '{command} {arguments}': {result}");
    }
}
