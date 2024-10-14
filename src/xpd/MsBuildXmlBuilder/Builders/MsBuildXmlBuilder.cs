using System.Xml.Linq;
using xpd.MsBuildXmlBuilder.Enums;

namespace xpd.MsBuildXmlBuilder.Builders;

internal sealed class MsBuildXmlBuilder
{
    private readonly XElement _project = new("Project");

    public MsBuildXmlBuilder AddPropertyGroup(params PropertyBuilder[] properties)
    {
        var builder = new PropertyGroupBuilder(properties.ToList());
        _project.Add(builder.Build());
        return this;
    }

    public MsBuildXmlBuilder AddPropertyGroup(Action<PropertyGroupBuilder> propertyGroupBuilder)
    {
        var builder = new PropertyGroupBuilder();
        propertyGroupBuilder(builder);
        _project.Add(builder.Build());
        return this;
    }

    public MsBuildXmlBuilder AddTarget(TargetName name, Action<TargetBuilder> targetBuilder)
    {
        var builder = new TargetBuilder(name);
        targetBuilder(builder);
        _project.Add(builder.Build());
        return this;
    }

    public override string ToString()
    {
        return new XDocument(_project).ToString();
    }
}
