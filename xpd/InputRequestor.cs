using Sharprompt;

namespace xpd;

public class InputRequestor : IInputRequestor
{
    public string? GetSolutionName()
    {
        return Prompt.Input<string?>("Enter solution name");
    }

    public string? GetProjectName(string defaultName)
    {
        return Prompt.Input<string?>("Enter project name", defaultName);
    }

    public List<string> GetFoldersToCreate()
    {
        var options = new[] { "src", "tests", "samples", "docs", "build", "config" };
        const int minimum = 0;
        return Prompt
            .MultiSelect("Create folders", options, minimum: minimum, defaultValues: options)
            .ToList();
    }
}
