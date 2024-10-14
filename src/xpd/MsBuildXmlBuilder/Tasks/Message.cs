using System.Xml.Serialization;
using xpd.MsBuildXmlBuilder.Attributes;
using xpd.MsBuildXmlBuilder.Enums;

namespace xpd.MsBuildXmlBuilder.Tasks;

[XmlRoot]
public sealed class Message : MsBuildTask
{
    [XmlAttribute]
    [XmlRequired]
    public string? Text { get; set; }

    [XmlAttribute]
    public MessageImportance? Importance { get; set; }
}
