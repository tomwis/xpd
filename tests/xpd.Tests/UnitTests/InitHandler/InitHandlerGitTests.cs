using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework;

namespace xpd.Tests.UnitTests.InitHandler;

public class InitHandlerGitTests : InitHandlerTestsBase
{
    [Test]
    public void GitRepositoryIsInitialized()
    {
        // Arrange
        var initHandler = GetSubject();

        // Act
        _ = initHandler.Parse(new Init());

        // Assert
        AssertCommandWasCalled(ProcessProvider, "git", "init");
    }

    [Test]
    public void GitIgnoreIsCreated()
    {
        // Arrange
        var initHandler = GetSubject();

        // Act
        _ = initHandler.Parse(new Init());

        // Assert
        AssertDotnetCommandWasCalled(ProcessProvider, "new gitignore");
    }

    [Test]
    public void GitIgnoreFileExists()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var expectedGitIgnorePath = mockFileSystem.Path.Combine(result.MainFolder!, ".gitignore");
        mockFileSystem.File.Exists(expectedGitIgnorePath).Should()
            .BeTrue();
    }

    [Test]
    public void GitIgnoreFileContainsXpdAddedEntries()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var gitIgnorePath = mockFileSystem.Path.Combine(result.MainFolder!, ".gitignore");
        var gitIgnoreContent = mockFileSystem.File.ReadAllText(gitIgnorePath);
        gitIgnoreContent.Should().Contain("/config/dotnet_tools_installed.txt");
    }
}
