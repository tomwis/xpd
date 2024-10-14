using System.Collections.ObjectModel;
using System.Xml.Linq;
using xpd.MsBuildXmlBuilder.Interfaces;

namespace xpd.MsBuildXmlBuilder.Builders;

internal sealed class PropertyGroupBuilder : Collection<PropertyBuilder>
{
    private readonly XElement _propertyGroup = new XElement("PropertyGroup");

    public PropertyGroupBuilder() { }

    public PropertyGroupBuilder(IList<PropertyBuilder> list)
        : base(list) { }

    public string? this[IPropertyName propertyName]
    {
        get => this.FirstOrDefault(i => i.Name.GetName() == propertyName.GetName())?.Value;
        set => Add(new PropertyBuilder(propertyName, value));
    }

    public PropertyGroupBuilder AddProperty(PropertyBuilder propertyBuilder)
    {
        Add(propertyBuilder);
        return this;
    }

    public XElement Build()
    {
        foreach (var property in this)
        {
            _propertyGroup.Add(property.Build());
        }

        return _propertyGroup;
    }
}
