using System.Reflection;
using System.Text.Json;
using CommandLine;
using FluentAssertions;
using NUnit.Framework;
using xpd.Services;

namespace xpd.convention_tests;

public class ProjectTypesTests
{
    [Test]
    public void ProjectTypesInConfigAreTheSameAsInInitHelperText()
    {
        var projectTypesInConfig = JsonSerializer.Deserialize<List<string>>(
            ResourceProvider.GetResource("project_types.json")
        );

        var optionAttribute = typeof(Init)
            .GetProperty(nameof(Init.ProjectType))!
            .GetCustomAttribute<OptionAttribute>()!;

        var projectTypesInInit = optionAttribute
            .HelpText.Split("Supported values: ")[1]
            .Split(',')
            .Select(item => item.Trim());

        projectTypesInConfig.Should().NotBeEmpty();
        projectTypesInInit
            .Should()
            .BeEquivalentTo(
                projectTypesInConfig,
                "because values in Init.ProjectType.HelpText should be in sync with Config/project_types.json"
            );
    }
}
