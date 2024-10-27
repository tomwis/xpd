using System.Xml.Linq;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Xml;

namespace xpd.Tests.Extensions;

public static class XDocumentAssertionsExtensions
{
    public static AndWhichConstraint<XDocumentAssertions, XElement> SetBasicProperties(
        this XDocumentAssertions xElementAssertions
    )
    {
        var andWhichConstraint = xElementAssertions.HaveElement("PropertyGroup");
        andWhichConstraint
            .Which.Should()
            .HaveElement("DirectoryBuildTargetsDir", "$(MSBuildThisFileDirectory)")
            .And.HaveElement(
                "ToolListFile",
                "$(DirectoryBuildTargetsDir)config/dotnet_tools_installed.txt"
            );

        return andWhichConstraint;
    }

    public static AndWhichConstraint<XElementAssertions, IEnumerable<XElement>> RestoreDotnetTools(
        this XElementAssertions xElementAssertions
    )
    {
        var andWhichConstraint = xElementAssertions.HaveElement("Exec", Exactly.Twice());
        andWhichConstraint.Which.First().Should().HaveAttribute("Command", "dotnet tool restore");

        return andWhichConstraint;
    }

    public static AndWhichConstraint<XElementAssertions, IEnumerable<XElement>> InstallHusky(
        this XElementAssertions xElementAssertions
    )
    {
        var andWhichConstraint = xElementAssertions.HaveElement("Exec", Exactly.Twice());
        andWhichConstraint
            .Which.Last()
            .Should()
            .HaveAttribute("Command", "dotnet husky install")
            .And.HaveAttribute("WorkingDirectory", "$(DirectoryBuildTargetsDir)");

        return andWhichConstraint;
    }

    public static AndWhichConstraint<XElementAssertions, XElement> SaveHuskyInstallToFile(
        this XElementAssertions xElementAssertions
    )
    {
        var andWhichConstraint = xElementAssertions.HaveElement("WriteLinesToFile");
        andWhichConstraint
            .Which.Should()
            .HaveAttribute("File", "$(ToolListFile)")
            .And.HaveAttribute("Lines", "Husky");

        return andWhichConstraint;
    }

    public static AndWhichConstraint<XElementAssertions, XElement> SetInstalledToolsFromCache(
        this XElementAssertions xElementAssertions
    )
    {
        var andWhichConstraint = xElementAssertions.HaveElement("ReadLinesFromFile");
        andWhichConstraint
            .Which.Should()
            .HaveAttribute("File", "$(ToolListFile)")
            .And.HaveElement("Output")
            .Which.Should()
            .HaveAttribute("TaskParameter", "Lines")
            .And.HaveAttribute("ItemName", "ToolLines");
        andWhichConstraint
            .And.HaveElement("PropertyGroup")
            .Which.Should()
            .HaveElement("HuskyInstalled")
            .Which.Should()
            .HaveAttribute("Condition", "'%(ToolLines.Identity)' == 'Husky'")
            .And.HaveValue("true");

        return andWhichConstraint;
    }

    public static AndWhichConstraint<XElementAssertions, XElement> CallHuskyInstallIfNotInstalled(
        this XElementAssertions xElementAssertions
    )
    {
        var andWhichConstraint = xElementAssertions.HaveElement("CallTarget");
        andWhichConstraint
            .Which.Should()
            .HaveAttribute("Targets", "HuskyRestoreAndInstall")
            .And.HaveAttribute("Condition", "'$(HUSKY)' != '0' AND '$(HuskyInstalled)' != 'true'");

        return andWhichConstraint;
    }

    public static AndWhichConstraint<
        GenericCollectionAssertions<XElement>,
        XElement
    > HaveElementWithFolder(this XDocumentAssertions xDocumentAssertions, string expectedFolder)
    {
        return xDocumentAssertions
            .HaveElement("ItemGroup", AtLeast.Once())
            .Which.Should()
            .Contain(element => element.Element("Folder") != null)
            .Which.Should()
            .HaveElement("Folder", AtLeast.Once())
            .Which.Should()
            .ContainSingle(x => x.Attribute("Include")!.Value == expectedFolder);
    }
}
