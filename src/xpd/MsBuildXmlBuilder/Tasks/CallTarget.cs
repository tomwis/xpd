using System.Xml;
using System.Xml.Serialization;
using xpd.MsBuildXmlBuilder.Attributes;
using xpd.MsBuildXmlBuilder.Enums;

namespace xpd.MsBuildXmlBuilder.Tasks;

[XmlRoot]
public sealed class CallTarget : MsBuildTask
{
    [XmlRequired]
    [XmlAttribute]
    public TargetName[]? Targets { get; set; }
}
