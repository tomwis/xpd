using System.IO.Abstractions;
using System.Text.Json;
using xpd.Constants;
using xpd.Enums;
using xpd.Models;
using xpd.MsBuildXmlBuilder.Builders;
using xpd.MsBuildXmlBuilder.Enums;
using xpd.MsBuildXmlBuilder.Extensions;
using xpd.MsBuildXmlBuilder.Models;
using xpd.MsBuildXmlBuilder.Properties;
using xpd.MsBuildXmlBuilder.Tasks;

namespace xpd.Services;

public class HuskyService(IFileSystem fileSystem, CommandService commandService)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly CommandService _commandService = commandService;

    public InitResult? InitializeHuskyHooks(string mainFolder)
    {
        _commandService.RunCommand(
            "dotnet",
            "husky add pre-commit -c \"dotnet husky run --group pre-commit\"",
            mainFolder
        );

        var taskRunnerPath = _fileSystem.Path.Combine(mainFolder, ".husky", "task-runner.json");
        if (!_fileSystem.File.Exists(taskRunnerPath))
        {
            Console.WriteLine(
                "Warning: .husky/task-runner.json doesn't exist. Git hooks were not added."
            );

            return InitResult.WithError(InitError.HuskyTaskRunnerMissing);
        }

        var taskRunnerJson = _fileSystem.File.ReadAllText(taskRunnerPath);
        var taskRunner = JsonSerializer.Deserialize<TaskRunner>(taskRunnerJson);

        if (taskRunner is null)
        {
            Console.WriteLine(
                "Warning: task-runner.json couldn't be parsed. Git hooks were not added."
            );

            return InitResult.WithError(InitError.HuskyTaskRunnerError);
        }

        taskRunner.Tasks.Clear();
        taskRunner.Tasks.Add(
            new TaskRunnerTask
            {
                Name = "format-staged-files-with-csharpier",
                Group = "pre-commit",
                Command = "dotnet",
                Arguments = ["csharpier", "${staged}"],
                Include = ["**/*.cs"],
            }
        );

        var options = new JsonSerializerOptions { WriteIndented = true };
        taskRunnerJson = JsonSerializer.Serialize(taskRunner, options);
        _fileSystem.File.WriteAllText(taskRunnerPath, taskRunnerJson);

        return null;
    }

    public void InitializeHuskyRestoreTarget(string mainFolder)
    {
        var msBuildXmlBuilder = new MsBuildXmlBuilder.Builders.MsBuildXmlBuilder();
        var toolListFileValue =
            CustomProperty.DirectoryBuildTargetsDir.ToUnevaluatedValue()
            + Path.Combine(OptionalFoldersConstants.ConfigDir, FileConstants.DotnetToolsInstalled);
        var messageTagValue =
            $"[{FileConstants.DirectoryBuildTargets}][{MsBuildProperty.MSBuildProjectName.ToUnevaluatedValue()}]";

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

        var directoryBuildTargetsFile = Path.Combine(
            mainFolder,
            FileConstants.DirectoryBuildTargets
        );
        var contents = msBuildXmlBuilder.ToString();
        _fileSystem.File.WriteAllText(directoryBuildTargetsFile, contents);
    }

    private static void SetDotnetToolsRestoreAndInstallTarget(TargetBuilder target)
    {
        const string outputItemName = "ToolLines";
        const string huskyInstalledTrue = "true";
        var messageTag = CustomProperty.MessageTag.ToUnevaluatedValue();
        var outputItemIdentity = $"%({outputItemName}.Identity)";

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
            .AddMessage($"{messageTag} Tool: {outputItemIdentity}")
            .AddPropertyGroup(
                new PropertyBuilder(
                    CustomProperty.HuskyInstalled,
                    huskyInstalledTrue
                ).WithCondition(Condition.Equals(outputItemIdentity, "Husky"))
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
                                huskyInstalledTrue
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
