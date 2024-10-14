using System.Xml.Linq;

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

    public MsBuildXmlBuilder AddPropertyGroup(
        Action<PropertyGroupBuilder> propertyGroupBuilderAction
    )
    {
        var builder = new PropertyGroupBuilder();
        propertyGroupBuilderAction(builder);
        _project.Add(builder.Build());
        return this;
    }

    public MsBuildXmlBuilder AddTarget(Action<TargetBuilder> targetBuilderAction)
    {
        var builder = new TargetBuilder();
        targetBuilderAction(builder);
        _project.Add(builder.Build());
        return this;
    }

    public override string ToString()
    {
        return new XDocument(_project).ToString();
    }
}
