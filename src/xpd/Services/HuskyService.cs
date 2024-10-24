using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
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

internal class HuskyService(
    IFileSystem fileSystem,
    CommandService commandService,
    PathProvider pathProvider
)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly CommandService _commandService = commandService;
    private readonly PathProvider _pathProvider = pathProvider;

    public InitResult? InitializeHuskyHooks(IDirectoryInfo mainFolder)
    {
        _commandService.RunCommand(
            "dotnet",
            "husky add pre-commit -c \"dotnet husky run --group pre-commit\"",
            mainFolder.FullName
        );

        var taskRunnerPath = _pathProvider.HuskyTaskRunnerJson;
        if (!taskRunnerPath.Exists)
        {
            Console.WriteLine(
                "Warning: .husky/task-runner.json doesn't exist. Git hooks were not added."
            );

            return InitResult.WithError(InitError.HuskyTaskRunnerMissing);
        }

        var taskRunnerJson = _fileSystem.File.ReadAllText(taskRunnerPath.FullName);
        var taskRunner = JsonSerializer.Deserialize<TaskRunner>(taskRunnerJson);

        if (taskRunner is null)
        {
            Console.WriteLine(
                "Warning: task-runner.json couldn't be parsed. Git hooks were not added."
            );

            return InitResult.WithError(InitError.HuskyTaskRunnerError);
        }

        taskRunner.Tasks.Clear();
        taskRunner.Tasks.Add(GetCsharpierTask());
        taskRunner.Tasks.Add(GetBuildTask());

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        };
        taskRunnerJson = JsonSerializer.Serialize(taskRunner, options);
        _fileSystem.File.WriteAllText(taskRunnerPath.FullName, taskRunnerJson);

        return null;
    }

    private static TaskRunnerTask GetCsharpierTask() =>
        new()
        {
            Name = "format-staged-files-with-csharpier",
            Group = "pre-commit",
            Command = "dotnet",
            Arguments = ["csharpier", "${staged}"],
            Include = ["**/*.cs"],
        };

    private static TaskRunnerTask GetBuildTask() =>
        new()
        {
            Name = "build",
            Group = "pre-commit",
            Command = "dotnet",
            Arguments = ["build"],
        };

    public void InitializeHuskyRestoreTarget()
    {
        var msBuildXmlBuilder = new MsBuildXmlBuilder.Builders.MsBuildXmlBuilder();
        var toolListFileValue =
            CustomProperty.DirectoryBuildTargetsDir.ToUnevaluatedValue()
            + _fileSystem.Path.GetRelativePath(
                _pathProvider.MainFolder.FullName,
                _pathProvider.DotnetToolsInstalledFile.FullName
            );
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

        var contents = msBuildXmlBuilder.ToString();
        _fileSystem.File.WriteAllText(_pathProvider.DirectoryBuildTargetsFile.FullName, contents);
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
