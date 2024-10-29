using FluentAssertions;
using NUnit.Framework;
using xpd.Exceptions;

namespace xpd.Tests.UnitTests.InitHandler;

public class InitHandlerTests : InitHandlerTestsBase
{
    [Test]
    public void CreateDefaultFoldersInMainFolder()
    {
        // Arrange
        var initHandler = GetSubject();

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        result.CreatedFolders.Should().HaveCount(6);
        result
            .CreatedFolders.Should()
            .BeEquivalentTo(["src", "tests", "samples", "docs", "build", "config"]);
    }

    [Test]
    public void DotnetToolsManifestIsCreated()
    {
        // Arrange
        var initHandler = GetSubject();

        // Act
        _ = initHandler.Parse(new Init());

        // Assert
        AssertDotnetCommandWasCalled(ProcessProvider, "new tool-manifest");
    }

    [Test]
    public void DotnetToolsAreInstalled()
    {
        // Arrange
        var initHandler = GetSubject();

        // Act
        _ = initHandler.Parse(new Init());

        // Assert
        AssertDotnetCommandWasCalled(ProcessProvider, "tool install csharpier");
        AssertDotnetCommandWasCalled(ProcessProvider, "tool install husky");
        AssertDotnetCommandWasCalled(ProcessProvider, "husky install");
    }

    [Test]
    public void WhenRunCommandReturnError_ThenDoNotThrowException()
    {
        // Arrange
        var initHandler = GetSubject(processProvider: GetProcessProvider(errors: "any error"));

        // Act && Assert
        initHandler.Invoking(i => i.Parse(new Init())).Should().NotThrow<CommandException>();
    }

    [Test]
    public void WhenRunCommandHasNonZeroExitCode_ThenThrowException()
    {
        // Arrange
        var initHandler = GetSubject(processProvider: GetProcessProvider(exitCode: 1));

        // Act && Assert
        initHandler.Invoking(i => i.Parse(new Init())).Should().Throw<CommandException>();
    }
}
