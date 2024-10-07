using System.Diagnostics;
using xpd.Interfaces;

namespace xpd.Services;

public class ProcessProvider : IProcessProvider
{
    public IProcessWrapper Start(ProcessStartInfo startInfo)
    {
        return ProcessWrapper.Start(startInfo);
    }
}
