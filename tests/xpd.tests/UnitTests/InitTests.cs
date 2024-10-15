using System.Diagnostics;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using xpd.Enums;
using xpd.Exceptions;
using xpd.Interfaces;
using xpd.Models;
using xpd.tests.Extensions;
using static xpd.Constants.OptionalFoldersConstants;

namespace xpd.tests.UnitTests;

public class InitTests
{
    private IProcessProvider ProcessProvider { get; set; } = null!;

    [TestCase(null)]
    [TestCase("")]
    public void WhenSolutionNameArgumentIsEmpty_ThenAskForUserForInput(string solutionNameArg)
    {
        // Arrange
        const string expectedSolutionName = "SolutionNameFromUserInput";
        var init = GetSubject(expectedSolutionName);
        init.SolutionName = solutionNameArg;

        // Act
        var result = init.Parse(init);

        // Assert
        result.SolutionName.Should().Be(expectedSolutionName);
    }

    [Test]
    public void WhenSolutionNameArgumentIsNotEmpty_ThenDoNotAskForUserForInput()
    {
        // Arrange
        const string solutionNameFromArg = "ExpectedSolutionName";
        var init = GetSubjectForSolutionArgTest(solutionNameFromArg);

        // Act
        var result = init.Parse(init);

        // Assert
        result.SolutionName.Should().Be(solutionNameFromArg);
    }

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
    public void SolutionNameIsUsedAsProjectName()
    {
        // Arrange
        const string solutionName = "SomeSolution";
        var init = GetSubject(solutionName);

        // Act
        var result = init.Parse(init);

        // Assert
        result.ProjectName.Should().Be(solutionName);
    }

    [Test]
    public void CreateDefaultFoldersInMainFolder()
    {
        // Arrange
        var init = GetSubject();

        // Act
        var result = init.Parse(init);

        // Assert
        result.CreatedFolders.Should().HaveCount(6);
        result
            .CreatedFolders.Should()
            .BeEquivalentTo(["src", "tests", "samples", "docs", "build", "config"]);
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
        var init = GetSubject();

        // Act
        _ = init.Parse(init);

        // Assert
        AssertDotnetCommandWasCalled(ProcessProvider, "new tool-manifest");
    }

    [Test]
    public void WhenInitParseIsCalled_ThenDotnetToolsAreInstalled()
    {
        // Arrange
        var init = GetSubject();

        // Act
        _ = init.Parse(init);

        // Assert
        AssertDotnetCommandWasCalled(ProcessProvider, "tool install csharpier");
        AssertDotnetCommandWasCalled(ProcessProvider, "tool install husky");
        AssertDotnetCommandWasCalled(ProcessProvider, "husky install");
    }

    [Test]
    public void WhenInitParseIsCalled_ThenGitRepositoryIsInitialized()
    {
        // Arrange
        var init = GetSubject();

        // Act
        _ = init.Parse(init);

        // Assert
        AssertCommandWasCalled(ProcessProvider, "git", "init");
    }

    [Test]
    public void WhenInitParseIsCalled_ThenDirectoryBuildTargetsIsCorrectlyModified()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var init = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = init.Parse(init);

        // Assert
        var xml = GetXml(mockFileSystem, result.MainFolder!, "Directory.Build.targets");
        Console.WriteLine(xml);
        xml.Should().SetBasicProperties();
        var targetElements = xml.Should().HaveElement("Target", Exactly.Twice()).Which.ToList();

        targetElements
            .First()
            .Should()
            .HaveAttribute("Name", "DotnetToolsRestoreAndInstall")
            .And.HaveAttribute("BeforeTargets", "Restore;CollectPackageReferences")
            .And.SetInstalledToolsFromCache()
            .And.CallHuskyInstallIfNotInstalled();

        targetElements
            .Last()
            .Should()
            .HaveAttribute("Name", "HuskyRestoreAndInstall")
            .And.RestoreDotnetTools()
            .And.InstallHusky()
            .And.SaveHuskyInstallToFile();
    }

    [Test]
    public void TestProjectIsCreatedInTestsDir()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        const string solutionName = "SomeSolution";
        var init = GetSubject(solutionName, fileSystem: mockFileSystem);

        // Act
        var result = init.Parse(init);

        // Assert
        var expectedTestProjectPath = mockFileSystem.Path.GetFullPath(
            mockFileSystem.Path.Combine(solutionName, TestsDir, $"{solutionName}.Tests")
        );
        result.TestProjectPath.Should().Be(expectedTestProjectPath);
    }

    [Test]
    public void WhenRunCommandReturnsError_ThenThrowException()
    {
        // Arrange
        var init = GetSubject(processProvider: GetProcessProvider(errors: "any error"));

        // Act && Assert
        init.Invoking(i => i.Parse(init)).Should().Throw<CommandException>();
    }

    private static void AssertDotnetCommandWasCalled(
        IProcessProvider processProvider,
        string command
    )
    {
        AssertCommandWasCalled(processProvider, "dotnet", command);
    }

    private static void AssertCommandWasCalled(
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

    private static Init GetSubjectForSolutionArgTest(string solutionNameFromArg)
    {
        var fileSystem = new MockFileSystem();
        var currentDir = fileSystem.Directory.GetCurrentDirectory();
        var processProvider = GetProcessProvider(() =>
        {
            CreateTaskRunnerJson(fileSystem, currentDir, solutionNameFromArg);
            CreateTestsCsproj(fileSystem, currentDir, solutionNameFromArg, solutionNameFromArg);
        });

        var inputRequestor = Substitute.For<IInputRequestor>();
        return new Init(fileSystem, inputRequestor, processProvider)
        {
            SolutionName = solutionNameFromArg,
        };
    }

    private Init GetSubject(
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
            CreateTaskRunnerJson(fileSystem, currentDir, solutionName);
            CreateTestsCsproj(
                fileSystem,
                currentDir,
                solutionName,
                string.IsNullOrEmpty(projectName) ? solutionName : projectName
            );
        });

        var inputRequestor = Substitute.For<IInputRequestor>();
        inputRequestor.GetSolutionName().Returns(solutionName);
        return new Init(fileSystem, inputRequestor, processProvider)
        {
            Output = outputDir,
            SolutionName = null,
        };
    }

    private static IProcessProvider GetProcessProvider(Action? action = null, string? errors = null)
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

    private static string GetTaskRunnerJson() =>
        JsonSerializer.Serialize(new TaskRunner { Tasks = [] });

    private static void CreateTaskRunnerJson(
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

    private static void CreateTestsCsproj(
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
