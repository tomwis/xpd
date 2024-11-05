using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using NUnit.Framework;
using xpd.Models;
using xpd.Tests.Extensions;

namespace xpd.Tests.UnitTests.InitHandler;

public class InitHandlerGitHubActionsTests : InitHandlerTestsBase
{
    [Test]
    public void GitHubActionForBuildTestAndLintIsCreated()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem().WithExtensions();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var file = mockFileSystem
            .Path.Combine(result.MainFolder!, ".github", "workflows", "build-test-lint.yml")
            .ToFile();
        file.Exists.Should().BeTrue();
        file.ReadAllText()
            .Should()
            .NotContain("{RunsOn}")
            .And.NotContain("{DotnetWorkloadInstall}");
    }

    [TestCase("maui")]
    [TestCase("mauilib")]
    [TestCase("ios")]
    [TestCase("ios-tabbed")]
    [TestCase("ioslib")]
    [TestCase("iosbinding")]
    public void BuildTestLintAction_ForMauiOriOsApp_RunsOnMacOs(string projectType)
    {
        // Arrange
        const string expectedRunsOn = "runs-on: macos-latest";
        var mockFileSystem = new MockFileSystem().WithExtensions();
        var initHandler = GetSubject(fileSystem: mockFileSystem);
        var init = new Init { ProjectType = projectType };

        // Act
        var result = initHandler.Parse(init);

        // Assert
        mockFileSystem
            .Path.Combine(result.MainFolder!, ".github", "workflows", "build-test-lint.yml")
            .ToFile()
            .ReadAllText()
            .Should()
            .Contain(expectedRunsOn);
    }

    [Test]
    public void BuildTestLintAction_ForConsoleApp_RunsOnLinux()
    {
        // Arrange
        const string expectedRunsOn = "runs-on: ubuntu-latest";
        var mockFileSystem = new MockFileSystem().WithExtensions();
        var initHandler = GetSubject(fileSystem: mockFileSystem);
        var init = new Init { ProjectType = ProjectTypes.ConsoleApp };

        // Act
        var result = initHandler.Parse(init);

        // Assert
        mockFileSystem
            .Path.Combine(result.MainFolder!, ".github", "workflows", "build-test-lint.yml")
            .ToFile()
            .ReadAllText()
            .Should()
            .Contain(expectedRunsOn);
    }

    [TestCase("maui", "maui")]
    [TestCase("mauilib", "maui")]
    [TestCase("ios", "ios")]
    [TestCase("ios-tabbed", "ios")]
    [TestCase("ioslib", "ios")]
    [TestCase("iosbinding", "ios")]
    [TestCase("android", "android")]
    [TestCase("androidlib", "android")]
    [TestCase("android-bindinglib", "android")]
    public void BuildTestLintAction_ContainsDotnetWorkloadInstallStep_ForMobile(
        string projectType,
        string workloadToInstall
    )
    {
        // Arrange
        var expectedStep = $"dotnet workload install {workloadToInstall}";
        var mockFileSystem = new MockFileSystem().WithExtensions();
        var initHandler = GetSubject(fileSystem: mockFileSystem);
        var init = new Init { ProjectType = projectType };

        // Act
        var result = initHandler.Parse(init);

        // Assert
        mockFileSystem
            .Path.Combine(result.MainFolder!, ".github", "workflows", "build-test-lint.yml")
            .ToFile()
            .ReadAllText()
            .Should()
            .Contain(expectedStep);
    }

    [Test]
    public void BuildTestLintAction_DoesNotContainDotnetWorkloadInstallStep_ForNotMobile()
    {
        // Arrange
        const string notExpectedStep = "dotnet workload install";
        var mockFileSystem = new MockFileSystem().WithExtensions();
        var initHandler = GetSubject(fileSystem: mockFileSystem);
        var init = new Init { ProjectType = ProjectTypes.ConsoleApp };

        // Act
        var result = initHandler.Parse(init);

        // Assert
        mockFileSystem
            .Path.Combine(result.MainFolder!, ".github", "workflows", "build-test-lint.yml")
            .ToFile()
            .ReadAllText()
            .Should()
            .NotContain(notExpectedStep);
    }
}
