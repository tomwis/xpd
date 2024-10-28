using NUnit.Framework;

namespace xpd.Tests.UnitTests.InitHandler;

public class InitHandlerParamProjectTypeTests : InitHandlerTestsBase
{
    [TestCase("console")]
    [TestCase("maui")]
    public void WhenProjectTypeParameterIsProvided_ThenProjectOfSelectedTypeIsCreated(
        string projectType
    )
    {
        // Arrange
        const string solutionName = "solutionName";
        var initHandler = GetSubject(solutionName);

        // Act
        _ = initHandler.Parse(new Init { ProjectType = projectType });

        // Assert
        AssertDotnetCommandWasCalled(
            ProcessProvider,
            $"new {projectType} --output \"{solutionName}\""
        );
    }

    [Test]
    public void WhenProjectTypeParameterIsNotProvided_ThenConsoleProjectTypeIsCreated()
    {
        // Arrange
        const string solutionName = "solutionName";
        var initHandler = GetSubject(solutionName);

        // Act
        _ = initHandler.Parse(new Init());

        // Assert
        AssertDotnetCommandWasCalled(ProcessProvider, $"new console --output \"{solutionName}\"");
    }
}
