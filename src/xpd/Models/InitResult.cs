using xpd.Enums;

namespace xpd.Models;

public class InitResult
{
    private InitResult() { }

    private InitResult(
        string solutionName,
        string projectName,
        string solutionOutputDir,
        List<string> selectedFolders
    )
    {
        SolutionName = solutionName;
        ProjectName = projectName;
        SolutionOutputDir = solutionOutputDir;
        SelectedFolders = selectedFolders;
    }

    public InitError? Error { get; private init; }

    public static InitResult WithError(InitError error) => new() { Error = error };

    public static InitResult Success(
        string solutionName,
        string projectName,
        string solutionOutputDir,
        List<string> selectedFolders
    ) => new(solutionName, projectName, solutionOutputDir, selectedFolders);

    public string? SolutionName { get; private init; }
    public string? ProjectName { get; private init; }
    public string? SolutionOutputDir { get; private init; }
    public List<string>? SelectedFolders { get; private init; }
}