namespace xpd.SolutionModifier;

internal sealed class SolutionItem(string name, string path)
{
    public string Name { get; set; } = name;
    public string Path { get; set; } = path;

    public override string ToString()
    {
        var properties = GetType()
            .GetProperties()
            .Select(prop => $"{prop.Name}: {prop.GetValue(this)}");

        return $"{GetType().Name}({string.Join(Environment.NewLine, properties)})";
    }
}
