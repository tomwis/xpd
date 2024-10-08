using System.Diagnostics;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Xml.Linq;
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
        var init = GetSubject(solutionName);

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
        var init = GetSubject(solutionName: string.Empty);

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

        var init = GetSubject(
            solutionName,
            fileSystem: GetFileSystemWithSln(currentDir, solutionPath),
            outputDir: outputDir
        );

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

        var init = GetSubject(
            solutionName,
            fileSystem: GetFileSystemWithSln(currentDir, solutionPath)
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

        var mockFileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>(),
            new MockFileSystemOptions { CurrentDirectory = currentDir }
        );

        var init = GetSubject(solutionName, fileSystem: mockFileSystem, outputDir: outputDir);

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

        var mockFileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>(),
            new MockFileSystemOptions { CurrentDirectory = currentDir }
        );

        var init = GetSubject(solutionName, fileSystem: mockFileSystem);

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
        var init = GetSubject(solutionName, projectName);

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
        var init = GetSubject(solutionName, projectName);

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
        var init = GetSubject(foldersToCreate: selectedFolders);

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
        var mockFileSystem = new MockFileSystem();
        var init = GetSubject(solutionName, fileSystem: mockFileSystem, foldersToCreate: ["src"]);

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

    [Test]
    public void DirectoryBuildTargetsIsCreated()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var init = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = init.Parse(init);

        // Assert
        var expected = mockFileSystem.Path.Combine(result.MainFolder!, "Directory.Build.targets");
        mockFileSystem.File.Exists(expected).Should().BeTrue();
    }

    [Test]
    public void WhenDirectoryBuildTargetsIsCreated_ThenItHasProjectsTagAsRoot()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var init = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = init.Parse(init);

        // Assert
        var xdoc = GetXml(mockFileSystem, result.MainFolder!, "Directory.Build.targets");
        xdoc.Should().HaveRoot("Project");
    }

    [Test]
    public void WhenDirectoryPackagesPropsIsCreated_ThenItHasManagePackageVersionsCentrallyPropertySetToTrue()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var init = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = init.Parse(init);

        // Assert
        var xdoc = GetXml(mockFileSystem, result.MainFolder!, "Directory.Packages.props");
        xdoc.Should()
            .HaveElement("PropertyGroup")
            .Which.Should()
            .HaveElement("ManagePackageVersionsCentrally")
            .Which.Should()
            .HaveValue("true");
    }

    [TestCase("App")]
    [TestCase("Tests")]
    public void WhenDirectoryPackagesPropsIsCreated_ThenItHasItemGroupsWithLabels(
        string itemGroupLabel
    )
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var init = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = init.Parse(init);

        // Assert
        var xdoc = GetXml(mockFileSystem, result.MainFolder!, "Directory.Packages.props");
        xdoc.Should()
            .HaveElement("ItemGroup", Exactly.Twice())
            .Which.Should()
            .Contain(element => element.Attribute("Label")!.Value == itemGroupLabel);
    }

    [Test]
    public void WhenInitParseIsCalled_ThenDotnetToolsManifestIsCreated()
    {
        // Arrange
        var processProvider = GetProcessProvider();
        var init = GetSubject(processProvider: processProvider);

        // Act
        _ = init.Parse(init);

        // Assert
        AssertDotnetCommandWasCalled(processProvider, "new tool-manifest");
    }

    [Test]
    public void WhenInitParseIsCalled_ThenDotnetToolsAreInstalled()
    {
        // Arrange
        var processProvider = GetProcessProvider();
        var init = GetSubject(processProvider: processProvider);

        // Act
        _ = init.Parse(init);

        // Assert
        AssertDotnetCommandWasCalled(processProvider, "tool install csharpier");
    }

    private static void AssertDotnetCommandWasCalled(
        IProcessProvider processProvider,
        string command
    )
    {
        processProvider
            .Received(1)
            .Start(
                Arg.Is<ProcessStartInfo>(info =>
                    info.FileName == "dotnet" && info.Arguments == command
                )
            );
    }

    private static XDocument GetXml(
        MockFileSystem mockFileSystem,
        string mainFolder,
        string fileName
    )
    {
        var directoryPackagesPropsFile = mockFileSystem.Path.Combine(mainFolder, fileName);
        var fileContent = mockFileSystem.File.ReadAllText(directoryPackagesPropsFile);
        return XDocument.Parse(fileContent);
    }

    private static Init GetSubject(
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
