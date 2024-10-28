using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using NUnit.Framework;
using xpd.Tests.Extensions;

namespace xpd.Tests.UnitTests.InitHandler;

public class InitHandlerPropsAndTargetsFilesTests : InitHandlerTestsBase
{
    [Test]
    public void DirectoryBuildTargetsIsCreated()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var expected = mockFileSystem.Path.Combine(result.MainFolder!, "Directory.Build.targets");
        mockFileSystem.File.Exists(expected).Should().BeTrue();
    }

    [Test]
    public void WhenDirectoryBuildTargetsIsCreated_ThenItHasProjectsTagAsRoot()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var xdoc = GetXml(mockFileSystem, result.MainFolder!, "Directory.Build.targets");
        xdoc.Should().HaveRoot("Project");
    }

    [Test]
    public void WhenDirectoryPackagesPropsIsCreated_ThenItHasManagePackageVersionsCentrallyPropertySetToTrue()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

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
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

        // Assert
        var xdoc = GetXml(mockFileSystem, result.MainFolder!, "Directory.Packages.props");
        xdoc.Should()
            .HaveElement("ItemGroup", Exactly.Twice())
            .Which.Should()
            .Contain(element => element.Attribute("Label")!.Value == itemGroupLabel);
    }

    [Test]
    public void DirectoryBuildTargetsIsCorrectlyModified()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var initHandler = GetSubject(fileSystem: mockFileSystem);

        // Act
        var result = initHandler.Parse(new Init());

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
}
