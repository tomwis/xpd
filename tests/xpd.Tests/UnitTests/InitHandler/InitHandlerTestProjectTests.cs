using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using NUnit.Framework;
using static xpd.Constants.OptionalFoldersConstants;

namespace xpd.Tests.UnitTests.InitHandler;

public class InitHandlerTestProjectTests : InitHandlerTestsBase
{
    [Test]
    public void TestProjectIsCreatedInTestsDir()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        const string solutionName = "SomeSolution";
        var initHandler = GetSubject(solutionName, fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var expectedTestProjectPath = mockFileSystem.Path.GetFullPath(
            mockFileSystem.Path.Combine(solutionName, TestsDir, $"{solutionName}.Tests")
        );
        result.TestProjectPath.Should().Be(expectedTestProjectPath);
    }

    [TestCase("UnitTests")]
    [TestCase("IntegrationTests")]
    public void DefaultFoldersAreCreatedInTestsProject(string expectedFolder)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var expectedFolderPath = mockFileSystem.Path.Combine(
            result.TestProjectPath!,
            expectedFolder
        );
        mockFileSystem.Directory.Exists(expectedFolderPath).Should().BeTrue();
    }
}
