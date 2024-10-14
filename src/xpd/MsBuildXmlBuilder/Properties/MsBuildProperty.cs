using xpd.MsBuildXmlBuilder.Interfaces;

namespace xpd.MsBuildXmlBuilder.Properties;

internal sealed class MsBuildProperty : IPropertyName
{
    public static readonly MsBuildProperty MSBuildThisFileDirectory = new MsBuildProperty(
        "MSBuildThisFileDirectory"
    );
    public static readonly MsBuildProperty MSBuildProjectName = new MsBuildProperty(
        "MSBuildProjectName"
    );

    private readonly string _name;

    private MsBuildProperty(string name)
    {
        _name = name;
    }

    public string GetName() => _name;

    public override string ToString() => _name;
}
