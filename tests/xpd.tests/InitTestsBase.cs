using System.Diagnostics;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Text.Json;
using NSubstitute;
using xpd.Interfaces;
using xpd.Models;

namespace xpd.tests;

public abstract class InitTestsBase
{
    protected static Init GetSubject(
        string? solutionName = null,
        string? projectName = null,
        IFileSystem? fileSystem = null,
        string[]? foldersToCreate = null,
        string? outputDir = null,
        IProcessProvider? processProvider = null
    )
    {
        solutionName ??= "SomeSolution";
        fileSystem ??= new MockFileSystem();
        var currentDir = outputDir ?? fileSystem.Directory.GetCurrentDirectory();
        foldersToCreate ??= [];
        processProvider ??= GetProcessProvider(
            () => CreateTaskRunnerJson(fileSystem, currentDir, solutionName)
        );

        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(solutionName);
        inputRequestor.GetProjectName(Arg.Any<string>()).Returns(projectName);
        inputRequestor.GetFoldersToCreate().Returns(foldersToCreate.ToList());
        return new Init(fileSystem, inputRequestor, processProvider) { Output = outputDir };

        static string TaskRunnerJson() => JsonSerializer.Serialize(new TaskRunner { Tasks = [] });

        static void CreateTaskRunnerJson(
            IFileSystem fileSystem,
            string currentDir,
            string solutionName
        )
        {
            if (fileSystem is MockFileSystem mockFileSystem)
            {
                mockFileSystem.AddFile(
                    fileSystem.Path.Combine(currentDir, solutionName, ".husky", "task-runner.json"),
                    new MockFileData(TaskRunnerJson())
                );
            }
        }
    }

    protected static IProcessProvider GetProcessProvider(
        Action? action = null,
        string? errors = null
    )
    {
        var processProvider = Substitute.For<IProcessProvider>();
        var processWrapper = Substitute.For<IProcessWrapper>();
        processWrapper.StandardOutput.Returns(new StreamReader(new MemoryStream()));
        processWrapper.StandardError.Returns(new StreamReader(GetErrorStream(errors)));
        var configuredCall = processProvider
            .Start(Arg.Any<ProcessStartInfo>())
            .Returns(processWrapper);

        if (action is not null)
        {
            configuredCall.AndDoes(_ => action());
        }
        return processProvider;

        static MemoryStream GetErrorStream(string? errors = null)
        {
            var memoryStream = new MemoryStream();
            if (errors is not null)
            {
                var buffer = Encoding.UTF8.GetBytes(errors);
                memoryStream.Write(buffer, 0, buffer.Length);
                memoryStream.Position = 0;
            }

            return memoryStream;
        }
    }
}
