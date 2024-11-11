using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions.TestingHelpers;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using xpd.Exceptions;
using xpd.Tests.Assertions.Extensions;
using xpd.Tests.Extensions;

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
        AssertDotnetCommandWasCalled(ProcessProvider, "tool install versionize");
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

    [Test]
    public void WhenProjectIsCreated_ThenItHasNugetProperties()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem().WithExtensions();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var csproj = mockFileSystem
            .Path.Combine(
                result.MainFolder!,
                "src",
                result.ProjectName!,
                $"{result.ProjectName}.csproj"
            )
            .ToFile()
            .ReadAllText();
        var xml = XDocument.Parse(csproj);

        xml.Root!.Should()
            .HaveElement("PropertyGroup")
            .Which.Should()
            .HaveElement("Version")
            .And.HaveElement("PackageId")
            .And.HaveElement("PackageOutputPath");
    }

    [Test]
    public void ReleaseNugetScriptIsCreated()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem().WithExtensions();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var script = mockFileSystem
            .Path.Combine(result.MainFolder!, "build", "release-nuget.sh")
            .ToFile()
            .ReadAllText();

        script.Should().Contain("prepare()").And.Contain("publish()");
        script.Should().NotContain("{ProjectPath}").And.NotContain("{ProjectCsprojFileName}");
    }

    [Test]
    [Platform(Exclude = "Win")]
    [SuppressMessage(
        "Interoperability",
        "CA1416:Validate platform compatibility",
        Justification = "GetUnixFileMode works on Unix only"
    )]
    public void ReleaseNugetScript_ShouldBeExecutable()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem().WithExtensions();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var scriptPath = mockFileSystem.Path.Combine(
            result.MainFolder!,
            "build",
            "release-nuget.sh"
        );

        scriptPath.ToFile().Should().BeExecutable();
    }

    [TestCase(".env")]
    [TestCase(".env.example")]
    public void EnvFilesAreCreated(string fileName)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem().WithExtensions();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        mockFileSystem
            .Path.Combine(result.MainFolder!, "config", fileName)
            .ToFile()
            .Exists.Should()
            .BeTrue();
    }

    [TestCase(".env")]
    [TestCase(".env.example")]
    public void EnvFilesContainNugetApiKey(string fileName)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem().WithExtensions();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        mockFileSystem
            .Path.Combine(result.MainFolder!, "config", fileName)
            .ToFile()
            .ReadAllText()
            .Should()
            .Be("NUGET_API_KEY=");
    }
}
