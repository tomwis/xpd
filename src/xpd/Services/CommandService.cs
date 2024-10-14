using System.Diagnostics;
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
