using System.IO.Abstractions;
using System.Xml.Linq;

namespace xpd.Services;

internal sealed class MsBuildService(IFileSystem fileSystem, PathProvider pathProvider)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly PathProvider _pathProvider = pathProvider;

    public void CreateDirectoryBuildTargets()
    {
        var doc = new XDocument(new XElement("Project"));
        _fileSystem.File.WriteAllText(
            _pathProvider.DirectoryBuildTargetsFile.FullName,
            doc.ToString()
        );
    }

    public void CreateDirectoryPackagesProps()
    {
        var doc = new XDocument(
            new XElement(
                "Project",
                new XElement(
                    "PropertyGroup",
                    new XElement("ManagePackageVersionsCentrally", "true")
                ),
                new XElement("ItemGroup", new XAttribute("Label", "App")),
                new XElement("ItemGroup", new XAttribute("Label", "Tests"))
            )
        );
        _fileSystem.File.WriteAllText(
            _pathProvider.DirectoryPackagesPropsFile.FullName,
            doc.ToString()
        );
    }

    public void MovePackageVersionsToDirectoryPackagesProps(IFileInfo csprojFilePath)
    {
        var csprojContent = _fileSystem.File.ReadAllText(csprojFilePath.FullName);
        var csprojXml = XDocument.Parse(csprojContent);
        var xmlRoot = csprojXml.Root!;

        const string includeAttr = "Include";
        const string versionAttr = "Version";
        var packageReferences = xmlRoot
            .Descendants("PackageReference")
            .Select(pr =>
            {
                var attributes = new
                {
                    Include = pr.Attribute(includeAttr)?.Value,
                    Version = pr.Attribute(versionAttr)?.Value,
                };
                pr.Attribute(versionAttr)?.Remove();
                return attributes;
            })
            .Where(pr => pr.Include is not null && pr.Version is not null)
            .ToList();

        _fileSystem.File.WriteAllText(csprojFilePath.FullName, csprojXml.ToString());

        var directoryPackagesPropsContent = _fileSystem.File.ReadAllText(
            _pathProvider.DirectoryPackagesPropsFile.FullName
        );
        var directoryPackagesPropsXml = XDocument.Parse(directoryPackagesPropsContent);
        var propsRoot = directoryPackagesPropsXml.Root!;
        var itemGroup = propsRoot
            .Elements("ItemGroup")
            .First(ig => ig.Attribute("Label")?.Value == "Tests");

        packageReferences.ForEach(pr =>
        {
            if (
                itemGroup
                    .Elements("PackageVersion")
                    .Any(element => ElementHasName(element, pr.Include!))
            )
            {
                return;
            }

            itemGroup.Add(
                new XElement(
                    "PackageVersion",
                    new XAttribute(includeAttr, pr.Include!),
                    new XAttribute(versionAttr, pr.Version!)
                )
            );
        });
        _fileSystem.File.WriteAllText(
            _pathProvider.DirectoryPackagesPropsFile.FullName,
            directoryPackagesPropsXml.ToString()
        );

        bool ElementHasName(XElement e, string name) => e.Attribute(includeAttr)!.Value == name;
    }
}
