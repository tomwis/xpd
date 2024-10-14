using System.Xml.Serialization;
using xpd.MsBuildXmlBuilder.Attributes;

namespace xpd.MsBuildXmlBuilder.Tasks;

[XmlRoot]
public sealed class WriteLinesToFile : MsBuildTask
{
    [XmlAttribute]
    [XmlRequired]
    public string? File { get; set; }

    [XmlAttribute]
    [XmlRequired]
    public string[]? Lines { get; set; }
}
