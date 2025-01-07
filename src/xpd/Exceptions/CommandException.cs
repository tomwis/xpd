namespace xpd.Exceptions;

internal sealed class CommandException(string message, int processExitCode) : Exception(message)
{
    public int ProcessExitCode { get; } = processExitCode;
}
