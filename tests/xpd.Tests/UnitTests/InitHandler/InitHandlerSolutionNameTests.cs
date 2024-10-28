using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using xpd.Enums;
using xpd.Interfaces;

namespace xpd.Tests.UnitTests.InitHandler;

public class InitHandlerSolutionNameTests : InitHandlerTestsBase
{
    [TestCase(null)]
    [TestCase("")]
    public void WhenSolutionNameArgumentIsEmpty_ThenAskForUserForInput(string? solutionNameArg)
    {
        // Arrange
        const string expectedSolutionName = "SolutionNameFromUserInput";
        var initHandler = GetSubject(expectedSolutionName);
        var init = new Init { SolutionName = solutionNameArg };

        // Act
        var result = initHandler.Parse(init);

        // Assert
        result.SolutionName.Should().Be(expectedSolutionName);
    }

    [Test]
    public void WhenSolutionNameArgumentIsNotEmpty_ThenDoNotAskForUserForInput()
    {
        // Arrange
        const string solutionNameFromArg = "ExpectedSolutionName";
        var initHandler = GetSubjectForSolutionArgTest(solutionNameFromArg);
        var init = new Init { SolutionName = solutionNameFromArg };

        // Act
        var result = initHandler.Parse(init);

        // Assert
        result.SolutionName.Should().Be(solutionNameFromArg);
    }

    [Test]
    public void WhenSolutionNameIsNotEmpty_ThenReturnSuccess()
    {
        // Arrange
        const string solutionName = "NonEmptySolutionName";
        var initHandler = GetSubject(solutionName);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        result.Error.Should().BeNull();
        result.SolutionName.Should().Be(solutionName);
    }

    [Test]
    public void WhenSolutionNameIsEmpty_ThenReturnSolutionNameRequiredError()
    {
        // Arrange
        var initHandler = GetSubject(solutionName: string.Empty);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        result.Error.Should().Be(InitError.SolutionNameRequired);
    }

    [Test]
    public void WhenFolderWithNameOfSolutionExists_AndOutputDirIsProvided_ThenReturnSolutionNameExistsError()
    {
        // Arrange
        const string solutionName = "SomeSolution";
        const string outputDir = "/OutputDir";
        const string currentDir = "/CurrentDir";
        const string solutionPath = $"{outputDir}/{solutionName}/{solutionName}.sln";

        var initHandler = GetSubject(
            solutionName,
            fileSystem: GetFileSystemWithSln(currentDir, solutionPath),
            outputDir: outputDir
        );

        var init = new Init { Output = outputDir };

        // Act
        var result = initHandler.Parse(init);

        // Assert
        result.Error.Should().Be(InitError.SolutionNameExists);
    }

    [Test]
    public void WhenFolderWithNameOfSolutionExists_AndCurrentDirIsUsed_ThenReturnSolutionNameExistsError()
    {
        // Arrange
        const string solutionName = "SomeSolution";
        const string currentDir = "/CurrentDir";
        const string solutionPath = $"{currentDir}/{solutionName}/{solutionName}.sln";

        var initHandler = GetSubject(
            solutionName,
            fileSystem: GetFileSystemWithSln(currentDir, solutionPath)
        );

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        result.Error.Should().Be(InitError.SolutionNameExists);
    }

    [Test]
    public void SolutionNameIsUsedAsProjectName()
    {
        // Arrange
        const string solutionName = "SomeSolution";
        var initHandler = GetSubject(solutionName);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        result.ProjectName.Should().Be(solutionName);
    }

    private static xpd.InitHandler GetSubjectForSolutionArgTest(string solutionNameFromArg)
    {
        var fileSystem = new MockFileSystem();
        var currentDir = fileSystem.Directory.GetCurrentDirectory();
        var processProvider = GetProcessProvider(() =>
        {
            CreateSolution(fileSystem, currentDir, solutionNameFromArg);
            CreateTaskRunnerJson(fileSystem, currentDir, solutionNameFromArg);
            CreateTestsCsproj(fileSystem, currentDir, solutionNameFromArg, solutionNameFromArg);
        });

        var inputRequester = Substitute.For<IInputRequester>();
        return new xpd.InitHandler(fileSystem, inputRequester, processProvider);
    }

    private static MockFileSystem GetFileSystemWithSln(string currentDir, string solutionPath)
    {
        return new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [solutionPath] = new MockFileData(string.Empty),
            },
            new MockFileSystemOptions { CurrentDirectory = currentDir }
        );
    }
}
