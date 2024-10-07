using System.Diagnostics;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using xpd.Enums;
using xpd.Interfaces;

namespace xpd.tests;

public class InitCommandTests
{
    [Test]
    public void WhenSolutionNameIsNotEmpty_ThenReturnSuccess()
    {
        // Arrange
        const string solutionName = "NonEmptySolutionName";
        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(solutionName);
        inputRequestor.GetFoldersToCreate().Returns(new List<string>());

        var init = new Init(new MockFileSystem(), inputRequestor, GetProcessProvider());

        // Act
        var result = init.Parse(init);

        // Assert
        result.Error.Should().BeNull();
        result.SolutionName.Should().Be(solutionName);
    }

    [Test]
    public void WhenSolutionNameIsEmpty_ThenReturnSolutionNameRequiredError()
    {
        // Arrange
        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(string.Empty);

        var init = new Init(
            new MockFileSystem(),
            inputRequestor,
            Substitute.For<IProcessProvider>()
        );

        // Act
        var result = init.Parse(init);

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
        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(solutionName);

        var init = new Init(
            GetFileSystemWithSln(currentDir, solutionPath),
            inputRequestor,
            Substitute.For<IProcessProvider>()
        )
        {
            Output = outputDir,
        };

        // Act
        var result = init.Parse(init);

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
        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(solutionName);

        var init = new Init(
            GetFileSystemWithSln(currentDir, solutionPath),
            inputRequestor,
            Substitute.For<IProcessProvider>()
        );

        // Act
        var result = init.Parse(init);

        // Assert
        result.Error.Should().Be(InitError.SolutionNameExists);
    }

    [Test]
    public void WhenOutputDirIsProvided_ThenUseItAsOutputForSolutionFolder()
    {
        // Arrange
        const string solutionName = "SomeSolution";
        const string outputDir = "/OutputDir";
        const string currentDir = "/CurrentDir";
        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(solutionName);
        inputRequestor.GetFoldersToCreate().Returns(new List<string>());

        var mockFileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>(),
            new MockFileSystemOptions { CurrentDirectory = currentDir }
        );

        var init = new Init(mockFileSystem, inputRequestor, GetProcessProvider())
        {
            Output = outputDir,
        };

        // Act
        var result = init.Parse(init);

        // Assert
        var expected = mockFileSystem.Path.Combine(outputDir, solutionName);
        result.SolutionOutputDir.Should().Be(expected);
    }

    [Test]
    public void WhenOutputDirIsNotProvided_ThenUseCurrentDirAsOutputForSolutionFolder()
    {
        // Arrange
        const string solutionName = "SomeSolution";
        const string currentDir = "/CurrentDir";
        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(solutionName);
        inputRequestor.GetFoldersToCreate().Returns(new List<string>());

        var mockFileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>(),
            new MockFileSystemOptions { CurrentDirectory = currentDir }
        );

        var init = new Init(mockFileSystem, inputRequestor, GetProcessProvider());

        // Act
        var result = init.Parse(init);

        // Assert
        var expected = mockFileSystem.Path.Combine(currentDir, solutionName);
        result.SolutionOutputDir.Should().Be(expected);
    }

    [Test]
    public void WhenProjectNameIsProvided_ThenUseIt()
    {
        // Arrange
        const string solutionName = "SomeSolution";
        const string projectName = "SomeProject";
        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(solutionName);
        inputRequestor.GetProjectName(Arg.Any<string>()).Returns(projectName);
        inputRequestor.GetFoldersToCreate().Returns(new List<string>());
        var init = new Init(new MockFileSystem(), inputRequestor, GetProcessProvider());

        // Act
        var result = init.Parse(init);

        // Assert
        result.ProjectName.Should().Be(projectName);
    }

    [TestCase(null)]
    [TestCase("")]
    public void WhenProjectNameIsNullOrEmpty_ThenUseSolutionNameAsProjectName(string? projectName)
    {
        // Arrange
        const string solutionName = "SomeSolution";
        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(solutionName);
        inputRequestor.GetProjectName(Arg.Any<string>()).Returns(projectName);
        inputRequestor.GetFoldersToCreate().Returns(new List<string>());
        var init = new Init(new MockFileSystem(), inputRequestor, GetProcessProvider());

        // Act
        var result = init.Parse(init);

        // Assert
        result.ProjectName.Should().Be(solutionName);
    }

    [TestCase(["src", "tests", "samples", "docs", "build", "config"])]
    [TestCase(["src", "tests", "samples", "docs", "build"])]
    [TestCase(["src", "tests", "samples", "docs"])]
    [TestCase(["src", "tests", "samples"])]
    [TestCase(["src", "tests"])]
    [TestCase(["src"])]
    [TestCase([])]
    public void WhenFolderIsSelected_ThenCreateItInRootFolder(params string[] selectedFolders)
    {
        // Arrange
        const string solutionName = "SomeSolution";
        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(solutionName);
        inputRequestor.GetFoldersToCreate().Returns(selectedFolders.ToList());
        var init = new Init(new MockFileSystem(), inputRequestor, GetProcessProvider());

        // Act
        var result = init.Parse(init);

        // Assert
        result.SelectedFolders.Should().HaveCount(selectedFolders.Length);
        result.SelectedFolders.Should().BeEquivalentTo(selectedFolders);
    }

    [Test]
    public void WhenSrcFolderIsCreated_ThenCreateSolutionInsideIt()
    {
        // Arrange
        const string solutionName = "SomeSolution";
        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(solutionName);
        inputRequestor.GetFoldersToCreate().Returns(["src"]);
        var mockFileSystem = new MockFileSystem();
        var init = new Init(mockFileSystem, inputRequestor, GetProcessProvider());

        // Act
        var result = init.Parse(init);

        // Assert
        var expected = mockFileSystem.Path.Combine(
            mockFileSystem.Directory.GetCurrentDirectory(),
            solutionName,
            "src"
        );
        result.SolutionOutputDir.Should().Be(expected);
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

    private static IProcessProvider GetProcessProvider()
    {
        var processProvider = Substitute.For<IProcessProvider>();
        var processWrapper = Substitute.For<IProcessWrapper>();
        processWrapper.StandardOutput.Returns(new StreamReader(new MemoryStream()));
        processProvider.Start(Arg.Any<ProcessStartInfo>()).Returns(processWrapper);
        return processProvider;
    }
}
