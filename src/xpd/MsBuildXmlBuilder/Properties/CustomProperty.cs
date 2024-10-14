using xpd.MsBuildXmlBuilder.Interfaces;

namespace xpd.MsBuildXmlBuilder.Properties;

internal sealed class CustomProperty : IPropertyName
{
    public static readonly CustomProperty DirectoryBuildTargetsDir = new CustomProperty(
        "DirectoryBuildTargetsDir"
    );
    public static readonly CustomProperty ToolListFile = new CustomProperty("ToolListFile");
    public static readonly CustomProperty HuskyInstalled = new CustomProperty("HuskyInstalled");
    public static readonly CustomProperty MessageTag = new CustomProperty("MessageTag");

    private readonly string _name;

    private CustomProperty(string name)
    {
        _name = name;
    }

    public string GetName() => _name;

    public override string ToString() => _name;
}
