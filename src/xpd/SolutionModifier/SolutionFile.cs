using System.Text;

namespace xpd.SolutionModifier;

internal class SolutionFile(string solutionFileContent)
{
    protected const string SolutionItemsSectionStart = "ProjectSection(SolutionItems) = preProject";
    protected const string SolutionItemsSectionEnd = "EndProjectSection";

    private readonly StringBuilder _builder = new(solutionFileContent);

    protected static string SolutionFolderGuid => "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";

    internal void AddSolutionFolder(SolutionFolder solutionFolder)
    {
        _builder.AppendLine(
            $"Project(\"{SolutionFolderGuid}\") = \"{solutionFolder.Name}\", \"{solutionFolder.Name}\", \"{solutionFolder.Id}\""
        );

        _builder.AppendLine($"\t{SolutionItemsSectionStart}");

        foreach (var item in solutionFolder.Items)
        {
            _builder.AppendLine($"\t\t{item.Name} = {item.Path}");
        }

        _builder.AppendLine($"\t{SolutionItemsSectionEnd}");
        _builder.AppendLine("EndProject");
    }

    public override string ToString()
    {
        return _builder.ToString();
    }
}
