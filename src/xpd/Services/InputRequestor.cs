using Sharprompt;
using xpd.Interfaces;

namespace xpd.Services;

public class InputRequestor : IInputRequestor
{
    public string? GetSolutionName()
    {
        return Prompt.Input<string?>("Enter solution name");
    }
}
