using System.Diagnostics;
using xpd.Exceptions;
using xpd.Interfaces;

namespace xpd.Services;

public class ProcessWrapper(Process process) : IProcessWrapper
{
    private readonly Process _process = process;

    public StreamReader StandardOutput => _process.StandardOutput;
    public StreamReader StandardError => _process.StandardError;

    public static IProcessWrapper Start(ProcessStartInfo startInfo)
    {
        var process = Process.Start(startInfo);
        if (process is null)
        {
            throw new ProcessException(
                $"Failed to start process. Is'{startInfo.FileName}' installed and in PATH?"
            );
        }

        return new ProcessWrapper(process);
    }

    public void WaitForExit()
    {
        _process.WaitForExit();
    }

    public void Dispose()
    {
        _process.Dispose();
    }
}
