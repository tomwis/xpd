namespace xpd.Interfaces;

public interface IProcessWrapper : IDisposable
{
    StreamReader StandardOutput { get; }
    void WaitForExit();
}
