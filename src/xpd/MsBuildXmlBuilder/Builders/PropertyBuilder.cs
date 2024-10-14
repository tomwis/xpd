using System.Xml.Linq;
using xpd.MsBuildXmlBuilder.Interfaces;

namespace xpd.MsBuildXmlBuilder.Builders;

internal sealed class PropertyBuilder(IPropertyName name, string? value)
    : ConditionalBuilder<PropertyBuilder>
{
    public IPropertyName Name { get; } = name;
    public string? Value { get; } = value;

    public XElement Build()
    {
        var element = new XElement(Name.GetName(), Value);
        return BuildElement(element);
    }
}
