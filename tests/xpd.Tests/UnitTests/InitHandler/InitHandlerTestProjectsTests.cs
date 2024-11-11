using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using static xpd.Constants.OptionalFoldersConstants;

namespace xpd.Tests.UnitTests.InitHandler;

public class InitHandlerTestProjectsTests : InitHandlerTestsBase
{
    [Test]
    public void TestProjectIsCreatedInTestsDir()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        const string solutionName = "SomeSolution";
        var initHandler = GetSubject(solutionName, fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var expectedTestProjectPath = mockFileSystem.Path.GetFullPath(
            mockFileSystem.Path.Combine(solutionName, TestsDir, $"{solutionName}.Tests")
        );
        result.TestProjectPath.Should().Be(expectedTestProjectPath);
    }

    [Test]
    public void ConventionTestProjectIsCreatedInTestsDir()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        const string solutionName = "SomeSolution";
        var initHandler = GetSubject(solutionName, fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var expectedTestProjectPath = mockFileSystem.Path.GetFullPath(
            mockFileSystem.Path.Combine(solutionName, TestsDir, $"{solutionName}.ConventionTests")
        );
        result.ConventionTestProjectPath.Should().Be(expectedTestProjectPath);
    }

    [TestCase("UnitTests")]
    [TestCase("IntegrationTests")]
    public void DefaultFoldersAreCreatedInTestsProject(string expectedFolder)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var expectedFolderPath = mockFileSystem.Path.Combine(
            result.TestProjectPath!,
            expectedFolder
        );
        mockFileSystem.Directory.Exists(expectedFolderPath).Should().BeTrue();
    }

    [Test]
    public void IntegrationTestsHasSetupFixtureClass()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var filePath = mockFileSystem.Path.Combine(
            result.TestProjectPath!,
            "IntegrationTests",
            "SetupFixture.cs"
        );
        mockFileSystem.FileInfo.New(filePath).Exists.Should().BeTrue();
    }

    [Test]
    public void SetupFixtureInIntegrationTestsHasCheckToPreventRunInGitHook()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var filePath = mockFileSystem.Path.Combine(
            result.TestProjectPath!,
            "IntegrationTests",
            "SetupFixture.cs"
        );
        var setupFixture = mockFileSystem.File.ReadAllText(filePath);
        AssertSetupFixtureHasGitHookCheck(setupFixture);
    }

    private void AssertSetupFixtureHasGitHookCheck(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var classDeclaration = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(cl => HasAttribute(cl, nameof(SetUpFixtureAttribute)));

        classDeclaration.Should().NotBeNull();
        var methodDeclaration = classDeclaration!
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(cl => HasAttribute(cl, nameof(OneTimeSetUpAttribute)));

        methodDeclaration.Should().NotBeNull();

        var ifStatement = methodDeclaration!
            .DescendantNodes()
            .OfType<IfStatementSyntax>()
            .FirstOrDefault(iss =>
                HasCondition(
                    iss,
                    SyntaxKind.EqualsEqualsToken,
                    "Environment.GetEnvironmentVariable(\"GIT_HOOK_EXECUTION\")",
                    "\"true\""
                )
            );

        ifStatement.Should().NotBeNull();
        ifStatement!
            .Statement.ToFullString()
            .Should()
            .Contain("throw new InvalidOperationException");
    }

    private static bool HasAttribute(
        MemberDeclarationSyntax memberDeclarationSyntax,
        string attributeName
    )
    {
        return memberDeclarationSyntax.AttributeLists.Any(attr =>
            attr.Attributes.Any(a =>
                a.Name is IdentifierNameSyntax identifier
                && identifier.Identifier.Text.Replace("Attribute", "")
                    == attributeName.Replace("Attribute", "")
            )
        );
    }

    private static bool HasCondition(
        IfStatementSyntax ifStatementSyntax,
        SyntaxKind syntaxKind,
        string left,
        string right
    )
    {
        return ifStatementSyntax
            .DescendantNodes()
            .OfType<BinaryExpressionSyntax>()
            .FirstOrDefault(bes =>
                bes.OperatorToken.IsKind(syntaxKind)
                && bes.Left.ToFullString().Trim() == left
                && bes.Right.ToFullString().Trim() == right
            )
            is not null;
    }
}
