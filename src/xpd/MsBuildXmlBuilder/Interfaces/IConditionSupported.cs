using xpd.MsBuildXmlBuilder.Models;

namespace xpd.MsBuildXmlBuilder.Interfaces;

internal interface IConditionSupported
{
    Condition? Condition { get; set; }
}
