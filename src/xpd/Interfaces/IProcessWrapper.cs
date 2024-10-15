namespace xpd.Interfaces;

public interface IProcessWrapper : IDisposable
{
    StreamReader StandardOutput { get; }
    StreamReader StandardError { get; }
    void WaitForExit();
}
