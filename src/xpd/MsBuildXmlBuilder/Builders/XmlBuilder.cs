using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace xpd.MsBuildXmlBuilder.Builders;

internal sealed class XmlBuilder<T>
    where T : new()
{
    private T Instance { get; } = new();

    public XmlBuilder<T> With<TProp>(Expression<Func<T, TProp>> propertyExpression, TProp value)
    {
        if (propertyExpression.Body is not MemberExpression memberExpression)
            throw new ArgumentException(
                $"Property expression is not valid. {propertyExpression} should refer to a property."
            );

        if (memberExpression.Member is not PropertyInfo propertyInfo)
            throw new ArgumentException(
                $"Expression {propertyExpression} does not refer to a property. Member type is {memberExpression.Member.MemberType}."
            );

        propertyInfo.SetValue(Instance, value);
        return this;
    }

    public XElement Build() => ToXml();

    private XElement ToXml()
    {
        var serializer = new XmlSerializer(typeof(T));
        var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

        using var stringWriter = new StringWriter();
        using var writer = XmlWriter.Create(stringWriter, settings);
        var ns = new XmlSerializerNamespaces([XmlQualifiedName.Empty]);
        serializer.Serialize(writer, Instance, ns);
        var xmlText = stringWriter.ToString();
        var xml = XDocument.Parse(xmlText);

        return xml.Root!;
    }
}
