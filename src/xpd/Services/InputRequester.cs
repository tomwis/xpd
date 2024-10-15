using Sharprompt;
using xpd.Interfaces;

namespace xpd.Services;

public class InputRequester : IInputRequester
{
    public string? GetSolutionName()
    {
        return Prompt.Input<string?>("Enter solution name");
    }
}
