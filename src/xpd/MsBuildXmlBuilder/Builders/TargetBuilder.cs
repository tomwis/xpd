using System.Xml.Linq;
using xpd.MsBuildXmlBuilder.Enums;
using xpd.MsBuildXmlBuilder.Interfaces;
using xpd.MsBuildXmlBuilder.Models;
using xpd.MsBuildXmlBuilder.Tasks;

namespace xpd.MsBuildXmlBuilder.Builders;

internal sealed class TargetBuilder
{
    private readonly XElement _target = new("Target");

    public TargetBuilder AddName(TargetName name)
    {
        _target.Add(new XAttribute("Name", name));
        return this;
    }

    public TargetBuilder AddCondition(Condition condition)
    {
        _target.Add(new XAttribute("Condition", condition));
        return this;
    }

    public TargetBuilder AddBeforeTargets(params TargetName[] beforeTargets)
    {
        _target.Add(new XAttribute("BeforeTargets", string.Join(";", beforeTargets)));
        return this;
    }

    public TargetBuilder AddMessage(
        string text,
        MessageImportance importance = MessageImportance.High
    )
    {
        var xElement = new XmlBuilder<Message>()
            .With(i => i.Text, text)
            .With(i => i.Importance, importance)
            .Build();
        _target.Add(xElement);
        return this;
    }

    public TargetBuilder AddExec(string command, string? workingDirectory = null)
    {
        var builder = new XmlBuilder<Exec>()
            .With(i => i.Command, command)
            .With(i => i.StandardOutputImportance, MessageImportance.Low)
            .With(i => i.StandardErrorImportance, MessageImportance.High);

        if (!string.IsNullOrEmpty(workingDirectory))
        {
            builder.With(i => i.WorkingDirectory, workingDirectory);
        }

        var xElement = builder.Build();
        _target.Add(xElement);
        return this;
    }

    public TargetBuilder AddReadLinesFromFile(string file, string outputItemName)
    {
        var xElement = new XmlBuilder<ReadLinesFromFile>()
            .With(i => i.File, file)
            .With(i => i.Output, Output.ReadLinesFromFile(outputItemName))
            .Build();

        _target.Add(xElement);
        return this;
    }

    public TargetBuilder AddWriteLinesToFile(string file, string lines)
    {
        var xElement = new XmlBuilder<WriteLinesToFile>()
            .With(i => i.File, file)
            .With(i => i.Lines, [lines])
            .Build();

        _target.Add(xElement);
        return this;
    }

    public TargetBuilder AddTask<T>(Func<XmlBuilder<T>, XmlBuilder<T>> taskBuilderFunc)
        where T : IMsBuildTask, new()
    {
        var builderFunc = taskBuilderFunc(new XmlBuilder<T>());
        var xElement = builderFunc.Build();
        _target.Add(xElement);
        return this;
    }

    public TargetBuilder AddPropertyGroup(params PropertyBuilder[] properties)
    {
        var builder = new PropertyGroupBuilder(properties.ToList());
        _target.Add(builder.Build());
        return this;
    }

    public XElement Build() => _target;
}
