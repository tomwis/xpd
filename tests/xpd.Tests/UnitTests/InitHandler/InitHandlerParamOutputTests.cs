using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using NUnit.Framework;

namespace xpd.Tests.UnitTests.InitHandler;

public class InitHandlerParamOutputTests : InitHandlerTestsBase
{
    [Test]
    public void WhenOutputDirIsProvided_ThenUseItAsOutputForSolutionFolder()
    {
        // Arrange
        const string solutionName = "SomeSolution";
        const string outputDir = "/OutputDir";
        const string currentDir = "/CurrentDir";

        var mockFileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>(),
            new MockFileSystemOptions { CurrentDirectory = currentDir }
        );

        var initHandler = GetSubject(
            solutionName,
            fileSystem: mockFileSystem,
            outputDir: outputDir
        );
        var init = new Init { Output = outputDir };

        // Act
        var result = initHandler.Parse(init);

        // Assert
        var expected = mockFileSystem.Path.Combine(outputDir, solutionName);
        result.MainFolder.Should().Be(expected);
    }

    [Test]
    public void WhenOutputDirIsNotProvided_ThenUseCurrentDirAsOutputForSolutionFolder()
    {
        // Arrange
        const string solutionName = "SomeSolution";
        const string currentDir = "/CurrentDir";

        var mockFileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>(),
            new MockFileSystemOptions { CurrentDirectory = currentDir }
        );

        var initHandler = GetSubject(solutionName, fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var expected = mockFileSystem.Path.Combine(currentDir, solutionName);
        result.MainFolder.Should().Be(expected);
    }

    [Test]
    public void WhenOutputDirStartsWithTildeSign_ThenExpandPath()
    {
        // Arrange
        const string solutionName = "SomeSolution";
        const string outputDirName = "OutputDir";
        const string outputDirPath = $"~/{outputDirName}";
        const string currentDir = "/CurrentDir";

        var mockFileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>(),
            new MockFileSystemOptions { CurrentDirectory = currentDir }
        );

        var initHandler = GetSubject(
            solutionName,
            fileSystem: mockFileSystem,
            outputDir: outputDirPath
        );
        var init = new Init { Output = outputDirPath };

        // Act
        var result = initHandler.Parse(init);

        // Assert
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var expected = mockFileSystem.Path.Combine(homeDirectory, outputDirName, solutionName);
        result.MainFolder.Should().Be(expected);
    }
}
