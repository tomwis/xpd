using FluentAssertions;
using NUnit.Framework;
using xpd.Models;

namespace xpd.tests.UnitTests;

public class ProjectTypeTests
{
    [TestCaseSource(nameof(TestCases))]
    public void WhenCallingToString_ThenReturnSupportedString(
        ProjectType projectType,
        string expected
    )
    {
        //Arrange & Act
        projectType.ToString().Should().Be(expected);
    }

    [Test]
    public void WhenCreatingProjectTypeWIthUnsupportedValue_ThenThrowInvalidOperationException()
    {
        //Arrange & Act
        Func<ProjectType> projectTypeFunc = () => "unsupported";

        // Assert
        projectTypeFunc.Should().Throw<InvalidOperationException>();
    }

    private static readonly object[] TestCases =
    {
        new object[] { ProjectTypes.ConsoleApp, "console" },
        new object[] { ProjectTypes.MauiApp, "maui" },
    };
}
