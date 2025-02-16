using System.IO.Abstractions;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using xpd.Interfaces;
using xpd.Models;
using xpd.Services;
using PathProvider = xpd.Tests.Utilities.PathProvider;

namespace xpd.Tests.IntegrationTests;

public class AllProjectTypesIntegrationTests
{
    private static IEnumerable<ProjectType> ProjectTypes =>
        [
            "console",
            "classlib",
            "maui",
            "mauilib",
            "ios",
            "ios-tabbed",
            "ioslib",
            "iosbinding",
            "android",
            "androidlib",
            "android-bindinglib",
        ];

    [Test]
    public void ProjectTypesInArrayAndConfigAreTheSame()
    {
        var projectTypesInConfig = JsonSerializer.Deserialize<List<string>>(
            ResourceProvider.GetResource("project_types.json")
        );

        ProjectTypes
            .Select(p => p.ToString())
            .Should()
            .BeEquivalentTo(
                projectTypesInConfig,
                "because project types for integration tests should be in sync with Config/project_types.json"
            );
    }

    [TestCaseSource(nameof(ProjectTypes))]
    public void WhenInitIsCalled_AndSupportedProjectTypeIsProvided_ThenCommandReturnsSuccess(
        ProjectType projectType
    )
    {
        // Arrange
        var outputPath = PathProvider.PrepareOutputDirForIntegrationTests(
            $"AllProjectTypesIntegrationTests-{projectType}"
        );
        var initHandler = GetSubject(new FileSystem(), new ProcessProvider(), new InputRequester());
        var init = new Init
        {
            Output = outputPath,
            SolutionName = "SolutionName",
            ProjectType = projectType,
        };

        // Act
        var result = initHandler.Parse(init);

        // Assert
        result.Error.Should().BeNull();
    }

    private InitHandler GetSubject(
        IFileSystem fileSystem,
        IProcessProvider processProvider,
        IInputRequester inputRequester
    )
    {
        return new InitHandler(fileSystem, inputRequester, processProvider);
    }
}
