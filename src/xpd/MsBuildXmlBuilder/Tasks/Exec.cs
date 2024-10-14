using System.Xml.Serialization;
using xpd.MsBuildXmlBuilder.Attributes;
using xpd.MsBuildXmlBuilder.Enums;

namespace xpd.MsBuildXmlBuilder.Tasks;

[XmlRoot]
public sealed class Exec : MsBuildTask
{
    [XmlAttribute]
    [XmlRequired]
    public string? Command { get; set; }

    [XmlAttribute]
    public MessageImportance? StandardOutputImportance { get; set; } = MessageImportance.Normal;

    [XmlAttribute]
    public MessageImportance? StandardErrorImportance { get; set; } = MessageImportance.Normal;

    [XmlAttribute]
    public string? WorkingDirectory { get; set; }
}
