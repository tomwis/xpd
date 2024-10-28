using xpd.SolutionModifier;

namespace xpd.Tests.Assertions.Models;

internal class SolutionFileForTest : SolutionFile
{
    public SolutionFileForTest(string content)
        : base(content)
    {
        Parse(content);
    }

    public List<SolutionFolder> SolutionFolders { get; } = new();

    private void Parse(string content)
    {
        using var reader = new StringReader(content);
        SolutionFolder? currentFolder = null;

        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();
            switch (line)
            {
                case not null when IsSolutionFolderStart(line):
                    var folderName = line.Split(['='])[1].Trim().Split(',')[0].Trim().Trim('"');
                    currentFolder = new SolutionFolder(folderName);
                    SolutionFolders.Add(currentFolder);
                    break;

                case not null when IsSolutionItemsStart(line):
                    var items = ReadSolutionItems(reader);
                    currentFolder?.Items.AddRange(items);
                    break;
            }
        }

        return;

        bool IsSolutionFolderStart(string line) =>
            line.StartsWith($"Project(\"{SolutionFolderGuid}\") = ");

        bool IsSolutionItemsStart(string line) => line.StartsWith(SolutionItemsSectionStart);
    }

    private static List<SolutionItem> ReadSolutionItems(StringReader reader)
    {
        List<SolutionItem> result = [];
        while (reader.ReadLine()?.Trim() is { } line && !line.StartsWith(SolutionItemsSectionEnd))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(['=']);
            if (parts.Length != 2)
                continue;

            var itemName = parts[0].Trim();
            var itemPath = parts[1].Trim();
            result.Add(new SolutionItem(itemName, itemPath));
        }

        return result;
    }
}
