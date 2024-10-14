using System.Xml.Serialization;
using xpd.MsBuildXmlBuilder.Attributes;
using xpd.MsBuildXmlBuilder.Models;

namespace xpd.MsBuildXmlBuilder.Tasks;

[XmlRoot]
public sealed class ReadLinesFromFile : MsBuildTask
{
    [XmlAttribute]
    [XmlRequired]
    public string? File { get; set; }

    [XmlRequired]
    public Output? Output { get; set; }
}
