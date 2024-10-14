using System.IO.Abstractions;
using xpd.MsBuildXmlBuilder.Builders;
using xpd.MsBuildXmlBuilder.Enums;
using xpd.MsBuildXmlBuilder.Extensions;
using xpd.MsBuildXmlBuilder.Models;
using xpd.MsBuildXmlBuilder.Properties;
using xpd.MsBuildXmlBuilder.Tasks;

namespace xpd.Services;

public class HuskyService(IFileSystem fileSystem)
{
    private readonly IFileSystem _fileSystem = fileSystem;

    public void InitializeHuskyRestoreTarget(string mainFolder)
    {
        var msBuildXmlBuilder = new MsBuildXmlBuilder.Builders.MsBuildXmlBuilder();
        var toolListFileValue =
            CustomProperty.DirectoryBuildTargetsDir.ToUnevaluatedValue()
            + Path.Combine("config", "dotnet_tools_installed.txt");
        var messageTagValue =
            $"[Directory.Build.targets][{MsBuildProperty.MSBuildProjectName.ToUnevaluatedValue()}]";

        msBuildXmlBuilder
            .AddPropertyGroup(pg =>
            {
                pg[CustomProperty.DirectoryBuildTargetsDir] =
                    MsBuildProperty.MSBuildThisFileDirectory.ToUnevaluatedValue();
                pg[CustomProperty.ToolListFile] = toolListFileValue;
                pg[CustomProperty.MessageTag] = messageTagValue;
            })
            .AddTarget(SetDotnetToolsRestoreAndInstallTarget)
            .AddTarget(SetHuskyRestoreAndInstallTarget);

        var directoryBuildTargetsFile = Path.Combine(mainFolder, "Directory.Build.targets");
        var contents = msBuildXmlBuilder.ToString();
        _fileSystem.File.WriteAllText(directoryBuildTargetsFile, contents);
    }

    private static void SetDotnetToolsRestoreAndInstallTarget(TargetBuilder target)
    {
        const string outputItemName = "ToolLines";
        var messageTag = CustomProperty.MessageTag.ToUnevaluatedValue();

        target
            .AddName(TargetName.DotnetToolsRestoreAndInstall)
            .AddBeforeTargets(TargetName.Restore, TargetName.CollectPackageReferences)
            .AddMessage(
                $"{messageTag} DirectoryBuildTargetsDir: {CustomProperty.DirectoryBuildTargetsDir.ToUnevaluatedValue()}"
            )
            .AddMessage(
                $"{messageTag} ToolListFile: {CustomProperty.ToolListFile.ToUnevaluatedValue()}"
            )
            .AddReadLinesFromFile(CustomProperty.ToolListFile.ToUnevaluatedValue(), outputItemName)
            .AddMessage($"{messageTag} Tool: %({outputItemName}.Identity)")
            .AddPropertyGroup(
                new PropertyBuilder(CustomProperty.HuskyInstalled, "true").WithCondition(
                    Condition.Equals($"%({outputItemName}.Identity)", "Husky")
                )
            )
            .AddMessage(
                $"{messageTag} HuskyInstalled: {CustomProperty.HuskyInstalled.ToUnevaluatedValue()}"
            )
            .AddTask<CallTarget>(task =>
                task.With(i => i.Targets, [TargetName.HuskyRestoreAndInstall])
                    .With(
                        i => i.Condition,
                        Condition.And(
                            Condition.NotEquals(
                                CustomProperty.Husky.ToUnevaluatedValue(),
                                0.ToString()
                            ),
                            Condition.NotEquals(
                                CustomProperty.HuskyInstalled.ToUnevaluatedValue(),
                                "true"
                            )
                        )
                    )
            );
    }

    private static void SetHuskyRestoreAndInstallTarget(TargetBuilder target)
    {
        target
            .AddName(TargetName.HuskyRestoreAndInstall)
            .AddExec("dotnet tool restore")
            .AddExec(
                "dotnet husky install",
                CustomProperty.DirectoryBuildTargetsDir.ToUnevaluatedValue()
            )
            .AddWriteLinesToFile(CustomProperty.ToolListFile.ToUnevaluatedValue(), "Husky");
    }
}
