namespace xpd;

public interface IInputRequestor
{
    string? GetSolutionName();
    string? GetProjectName(string defaultName);
    List<string> GetFoldersToCreate();
}
