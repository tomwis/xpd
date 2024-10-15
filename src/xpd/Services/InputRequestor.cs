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
        // No input for now to simplify process
        return defaultName;
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

        // No input for now to simplify process
        return options.ToList();
    }
}
