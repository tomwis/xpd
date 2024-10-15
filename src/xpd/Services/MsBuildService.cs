using System.IO.Abstractions;
using System.Xml.Linq;
using xpd.Constants;

namespace xpd.Services;

public class MsBuildService(IFileSystem fileSystem)
{
    private readonly IFileSystem _fileSystem = fileSystem;

    public void CreateDirectoryBuildTargets(string mainFolder)
    {
        var directoryBuildTargetsFile = _fileSystem.Path.Combine(
            mainFolder,
            FileConstants.DirectoryBuildTargets
        );
        var doc = new XDocument(new XElement("Project"));
        _fileSystem.File.WriteAllText(directoryBuildTargetsFile, doc.ToString());
    }

    public void CreateDirectoryPackagesProps(string mainFolder)
    {
        var directoryPackagesPropsFile = _fileSystem.Path.Combine(
            mainFolder,
            FileConstants.DirectoryPackagesProps
        );
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
        _fileSystem.File.WriteAllText(directoryPackagesPropsFile, doc.ToString());
    }

    public void MovePackageVersionsToDirectoryPackagesProps(
        string csprojFilePath,
        string directoryPackagePropsFilePath
    )
    {
        var csprojContent = _fileSystem.File.ReadAllText(csprojFilePath);
        var csprojXml = XDocument.Parse(csprojContent);
        var xmlRoot = csprojXml.Root!;

        var packageReferences = xmlRoot
            .Descendants("PackageReference")
            .Select(pr =>
            {
                var attributes = new
                {
                    Include = pr.Attribute("Include")?.Value,
                    Version = pr.Attribute("Version")?.Value,
                };
                pr.Attribute("Version")?.Remove();
                return attributes;
            })
            .Where(pr => pr.Include is not null && pr.Version is not null)
            .ToList();

        _fileSystem.File.WriteAllText(csprojFilePath, csprojXml.ToString());

        var directoryPackagesPropsContent = _fileSystem.File.ReadAllText(
            directoryPackagePropsFilePath
        );
        var directoryPackagesPropsXml = XDocument.Parse(directoryPackagesPropsContent);
        var propsRoot = directoryPackagesPropsXml.Root!;
        var itemGroup = propsRoot
            .Elements("ItemGroup")
            .First(ig => ig.Attribute("Label")?.Value == "Tests");

        packageReferences.ForEach(pr =>
        {
            itemGroup.Add(
                new XElement(
                    "PackageVersion",
                    new XAttribute("Include", pr.Include!),
                    new XAttribute("Version", pr.Version!)
                )
            );
        });
        _fileSystem.File.WriteAllText(
            directoryPackagePropsFilePath,
            directoryPackagesPropsXml.ToString()
        );
    }
}
