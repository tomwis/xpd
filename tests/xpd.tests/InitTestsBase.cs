using System.Diagnostics;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using NSubstitute;
using xpd.Interfaces;

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
        foldersToCreate ??= [];
        processProvider ??= GetProcessProvider();
        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(solutionName);
        inputRequestor.GetProjectName(Arg.Any<string>()).Returns(projectName);
        inputRequestor.GetFoldersToCreate().Returns(foldersToCreate.ToList());
        return new Init(fileSystem, inputRequestor, processProvider) { Output = outputDir };
    }

    protected static IProcessProvider GetProcessProvider()
    {
        var processProvider = Substitute.For<IProcessProvider>();
        var processWrapper = Substitute.For<IProcessWrapper>();
        processWrapper.StandardOutput.Returns(new StreamReader(new MemoryStream()));
        processProvider.Start(Arg.Any<ProcessStartInfo>()).Returns(processWrapper);
        return processProvider;
    }
}
