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
        string testProjectPath,
        string conventionTestProjectPath
    )
    {
        SolutionName = solutionName;
        ProjectName = projectName;
        MainFolder = mainFolder;
        CreatedFolders = createdFolders;
        TestProjectPath = testProjectPath;
        ConventionTestProjectPath = conventionTestProjectPath;
    }

    public InitError? Error { get; private init; }

    public static InitResult WithError(InitError error) => new() { Error = error };

    public static InitResult Success(
        string solutionName,
        string projectName,
        string mainFolder,
        List<string> createdFolders,
        string testProjectPath,
        string conventionTestProjectPath
    ) =>
        new(
            solutionName,
            projectName,
            mainFolder,
            createdFolders,
            testProjectPath,
            conventionTestProjectPath
        );

    public string? SolutionName { get; }
    public string? ProjectName { get; }
    public string? TestProjectPath { get; }
    public string? ConventionTestProjectPath { get; }
    public string? MainFolder { get; }
    public List<string>? CreatedFolders { get; }
}
