using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using xpd.MsBuildXmlBuilder.Attributes;

namespace xpd.MsBuildXmlBuilder.Builders;

internal sealed class XmlBuilder<T>
    where T : new()
{
    private T Instance { get; } = new();

    public XmlBuilder<T> With<TProp>(Expression<Func<T, TProp>> propertyExpression, TProp value)
    {
        if (propertyExpression.Body is not MemberExpression memberExpression)
            throw new ArgumentException("Property expression is not valid.");

        if (memberExpression.Member is not PropertyInfo propertyInfo)
            throw new ArgumentException("Expression does not refer to a property.");

        propertyInfo.SetValue(Instance, value);
        return this;
    }

    public XElement Build() => ToXml();

    private XElement ToXml()
    {
        VerifyRequiredAttribute(Instance!);

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

    private static void VerifyRequiredAttribute(object instance)
    {
        var propertyInfos = instance
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (var propertyInfo in propertyInfos)
        {
            var attribute = propertyInfo.GetCustomAttribute<XmlRequiredAttribute>();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (attribute is null)
                continue;

            var value = propertyInfo.GetValue(instance);
            if (value is null)
            {
                throw new InvalidOperationException(
                    $"Property '{propertyInfo.Name}' must be set before building the XML."
                );
            }

            if (IsComplexType(propertyInfo.PropertyType))
            {
                VerifyRequiredAttribute(value);
            }
        }

        return;

        static bool IsComplexType(Type type)
        {
            return type is { IsPrimitive: false, IsEnum: false }
                && type != typeof(string)
                && type != typeof(decimal)
                && type != typeof(DateTime)
                && type != typeof(TimeSpan)
                && type != typeof(Guid);
        }
    }
}
