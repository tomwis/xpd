using Sharprompt;
using xpd.Constants;
using xpd.Interfaces;

namespace xpd.Services;

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
        var options = new[]
        {
            OptionalFoldersConstants.SrcDir,
            OptionalFoldersConstants.TestsDir,
            OptionalFoldersConstants.SamplesDir,
            OptionalFoldersConstants.DocsDir,
            OptionalFoldersConstants.BuildDir,
            OptionalFoldersConstants.ConfigDir,
        };

        const int minimum = 0;
        return Prompt
            .MultiSelect("Create folders", options, minimum: minimum, defaultValues: options)
            .ToList();
    }
}
