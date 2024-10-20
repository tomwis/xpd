namespace xpd.SolutionModifier;

internal sealed class SolutionFolder(string name)
{
    public string Name { get; set; } = name;
    public string Id { get; set; } = Guid.NewGuid().ToString().ToUpper();
    public List<SolutionItem> Items { get; } = new List<SolutionItem>();

    public void AddItem(SolutionItem item)
    {
        Items.Add(item);
    }
}
