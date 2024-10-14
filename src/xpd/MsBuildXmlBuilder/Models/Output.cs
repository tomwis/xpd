using System.Xml.Serialization;
using xpd.MsBuildXmlBuilder.Attributes;

namespace xpd.MsBuildXmlBuilder.Models;

public sealed class Output
{
    private Output() { }

    [XmlAttribute]
    [XmlRequired]
    public string? TaskParameter { get; set; }

    [XmlAttribute]
    [XmlRequired]
    public string? ItemName { get; set; }

    public static Output ReadLinesFromFile(string itemName) =>
        new Output { TaskParameter = "Lines", ItemName = itemName };
}
