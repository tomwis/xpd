using System.Diagnostics;

namespace xpd.Interfaces;

public interface IProcessProvider
{
    IProcessWrapper Start(ProcessStartInfo startInfo);
}
