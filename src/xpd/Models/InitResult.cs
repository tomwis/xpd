using xpd.Enums;

namespace xpd.Models;

public class InitResult
{
    private InitResult() { }

    private InitResult(
        string solutionName,
        string projectName,
        string mainFolder,
        List<string> createdFolders,
        string solutionOutputDir,
        string testProjectPath
    )
    {
        SolutionName = solutionName;
        ProjectName = projectName;
        MainFolder = mainFolder;
        CreatedFolders = createdFolders;
        SolutionOutputDir = solutionOutputDir;
        TestProjectPath = testProjectPath;
    }

    public InitError? Error { get; private init; }

    public static InitResult WithError(InitError error) => new() { Error = error };

    public static InitResult Success(
        string solutionName,
        string projectName,
        string mainFolder,
        List<string> createdFolders,
        string solutionOutputDir,
        string testProjectPath
    ) =>
        new(
            solutionName,
            projectName,
            mainFolder,
            createdFolders,
            solutionOutputDir,
            testProjectPath
        );

    public string? SolutionName { get; }
    public string? ProjectName { get; }
    public string? SolutionOutputDir { get; }
    public string? TestProjectPath { get; }
    public string? MainFolder { get; }
    public List<string>? CreatedFolders { get; }
}
