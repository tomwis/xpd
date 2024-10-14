using System.Xml.Linq;
using xpd.MsBuildXmlBuilder.Models;

namespace xpd.MsBuildXmlBuilder.Builders;

internal abstract class ConditionalBuilder<T>
    where T : ConditionalBuilder<T>
{
    public Condition? Condition { get; private set; }

    public T WithCondition(Condition condition)
    {
        Condition = condition;
        return (T)this;
    }

    protected XElement BuildElement(XElement element)
    {
        if (Condition is not null)
        {
            element.Add(new XAttribute("Condition", Condition));
        }
        return element;
    }
}
