using System.Diagnostics;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using NSubstitute;
using xpd.Interfaces;
using xpd.Models;
using static xpd.Constants.OptionalFoldersConstants;

namespace xpd.Tests.UnitTests.InitHandler;

public abstract class InitHandlerTestsBase
{
    protected IProcessProvider ProcessProvider { get; set; } = null!;

    protected static void AssertDotnetCommandWasCalled(
        IProcessProvider processProvider,
        string command
    )
    {
        AssertCommandWasCalled(processProvider, "dotnet", command);
    }

    protected static void AssertCommandWasCalled(
        IProcessProvider processProvider,
        string command,
        string arguments
    )
    {
        processProvider
            .Received(1)
            .Start(
                Arg.Is<ProcessStartInfo>(info =>
                    info.FileName == command && info.Arguments == arguments
                )
            );
    }

    protected static XDocument GetXml(
        MockFileSystem mockFileSystem,
        string mainFolder,
        string fileName
    )
    {
        var directoryPackagesPropsFile = mockFileSystem.Path.Combine(mainFolder, fileName);
        var fileContent = mockFileSystem.File.ReadAllText(directoryPackagesPropsFile);
        return XDocument.Parse(fileContent);
    }

    protected xpd.InitHandler GetSubject(
        string? solutionName = null,
        string? projectName = null,
        IFileSystem? fileSystem = null,
        string? outputDir = null,
        IProcessProvider? processProvider = null
    )
    {
        solutionName ??= "SomeSolution";
        fileSystem ??= new MockFileSystem();
        var currentDir = outputDir ?? fileSystem.Directory.GetCurrentDirectory();
        ProcessProvider = processProvider ??= GetProcessProvider(() =>
        {
            CreateSolution(fileSystem, currentDir, solutionName);
            CreateTaskRunnerJson(fileSystem, currentDir, solutionName);
            CreateTestsCsproj(
                fileSystem,
                currentDir,
                solutionName,
                string.IsNullOrEmpty(projectName) ? solutionName : projectName
            );
        });

        var inputRequester = Substitute.For<IInputRequester>();
        inputRequester.GetSolutionName().Returns(solutionName);
        return new xpd.InitHandler(fileSystem, inputRequester, processProvider);
    }

    protected static IProcessProvider GetProcessProvider(
        Action? action = null,
        string? errors = null,
        int? exitCode = null
    )
    {
        var processProvider = Substitute.For<IProcessProvider>();
        var processWrapper = Substitute.For<IProcessWrapper>();
        processWrapper.StandardOutput.Returns(new StreamReader(new MemoryStream()));
        processWrapper.StandardError.Returns(new StreamReader(GetErrorStream(errors)));

        if (exitCode is not null)
        {
            processWrapper.ExitCode.Returns(exitCode.Value);
        }

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

    private static string GetTaskRunnerJson() =>
        JsonSerializer.Serialize(new TaskRunner { Tasks = [] });

    protected static void CreateTaskRunnerJson(
        IFileSystem fileSystem,
        string currentDir,
        string solutionName
    )
    {
        if (fileSystem is MockFileSystem mockFileSystem)
        {
            mockFileSystem.AddFile(
                fileSystem.Path.Combine(currentDir, solutionName, ".husky", "task-runner.json"),
                new MockFileData(GetTaskRunnerJson())
            );
        }
    }

    protected static void CreateSolution(
        IFileSystem fileSystem,
        string currentDir,
        string solutionName
    )
    {
        if (fileSystem is MockFileSystem mockFileSystem)
        {
            mockFileSystem.AddFile(
                fileSystem.Path.Combine(currentDir, solutionName, $"{solutionName}.sln"),
                new MockFileData("Microsoft Visual Studio Solution File, Format Version 12.00")
            );
        }
    }

    protected static void CreateTestsCsproj(
        IFileSystem fileSystem,
        string currentDir,
        string solutionName,
        string projectName
    )
    {
        if (fileSystem is not MockFileSystem mockFileSystem)
        {
            return;
        }

        string testsCsproj = $"{projectName}.Tests.csproj";
        var csproj = new XDocument(new XElement("Project"));
        var testProjectPath = Path.Combine(
            currentDir,
            solutionName,
            TestsDir,
            $"{projectName}.Tests",
            testsCsproj
        );

        mockFileSystem.AddFile(testProjectPath, new MockFileData(csproj.ToString()));
    }
}
