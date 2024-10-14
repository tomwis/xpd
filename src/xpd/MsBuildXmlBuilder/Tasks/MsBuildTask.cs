using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CommandLine;
using xpd.MsBuildXmlBuilder.Attributes;
using xpd.MsBuildXmlBuilder.Interfaces;
using xpd.MsBuildXmlBuilder.Models;

namespace xpd.MsBuildXmlBuilder.Tasks;

public abstract class MsBuildTask : IMsBuildTask, IXmlSerializable
{
    [XmlAttribute]
    public Condition? Condition { get; set; }

    public XmlSchema? GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        throw new NotSupportedException();
    }

    public void WriteXml(XmlWriter writer)
    {
        WriteXml(writer, this);
    }

    private void WriteXml(XmlWriter writer, object instance)
    {
        var propertyInfos = instance
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in propertyInfos)
        {
            var xmlIgnore = property.GetCustomAttribute<XmlIgnoreAttribute>();
            if (xmlIgnore is not null)
                continue;

            var xmlRequired = property.GetCustomAttribute<XmlRequiredAttribute>();
            var value = property.GetValue(instance);
            if (xmlRequired is not null && value is null)
                throw new InvalidOperationException($"The property '{property.Name}' is required.");

            if (xmlRequired is null && value is null)
                continue;

            var xmlAttribute = property.GetCustomAttribute<XmlAttributeAttribute>();
            if (xmlAttribute is not null)
            {
                if (property.PropertyType.IsArray && value is IEnumerable values)
                {
                    value = string.Join(";", values.Cast<object>());
                }

                writer.WriteAttributeString(property.Name, value?.ToString());
                continue;
            }

            writer.WriteStartElement(property.Name);
            WriteXml(writer, value!);
            writer.WriteEndElement();
        }
    }
}
