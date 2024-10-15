using System.IO.Abstractions;
using System.Xml.Linq;
using CommandLine;
using xpd.Enums;
using xpd.Interfaces;
using xpd.Models;
using xpd.Services;

namespace xpd;

[Verb("init")]
public class Init(
    IFileSystem fileSystem,
    IInputRequestor inputRequestor,
    IProcessProvider processProvider
)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly IInputRequestor _inputRequestor = inputRequestor;
    private readonly IProcessProvider _processProvider = processProvider;
    private readonly CommandService _commandService = new(processProvider);

    public Init()
        : this(new FileSystem(), new InputRequestor(), new ProcessProvider()) { }

    [Option('o', "output", Required = false, HelpText = "Parent folder for solution.")]
    public string? Output { get; set; }

    public InitResult Parse(Init args)
    {
        var solutionName = _inputRequestor.GetSolutionName();
        if (string.IsNullOrEmpty(solutionName))
        {
            Console.WriteLine("Solution name is required.");
            return InitResult.WithError(InitError.SolutionNameRequired);
        }

        var outputDir = args.Output ?? _fileSystem.Directory.GetCurrentDirectory();
        var solutionPath = _fileSystem.Path.Combine(outputDir, solutionName);
        var solutionDirectoryInfo = _fileSystem.DirectoryInfo.New(solutionPath);
        if (solutionDirectoryInfo.Exists)
        {
            Console.WriteLine(
                $"Directory '{solutionName}' already exists in current directory ({outputDir})."
            );
            return InitResult.WithError(InitError.SolutionNameExists);
        }

        var projectName = _inputRequestor.GetProjectName(solutionName);
        if (string.IsNullOrEmpty(projectName))
        {
            Console.WriteLine("Project name will be the same as solution name.");
            projectName = solutionName;
        }

        var selectedFolders = _inputRequestor.GetFoldersToCreate();
        var mainFolder = _fileSystem.Path.Combine(outputDir, solutionName);
        CreateFolders(mainFolder, selectedFolders);

        string solutionOutputDir = selectedFolders.Contains("src")
            ? _fileSystem.Path.Combine(outputDir, solutionName, "src")
            : mainFolder;

        string testsDir = selectedFolders.Contains("tests")
            ? _fileSystem.Path.Combine(mainFolder, "tests")
            : mainFolder;

        CreateProjectAndSolution(solutionOutputDir, solutionName, projectName);
        (string testProjectName, string testProjectPath) = CreateTestProject(
            solutionOutputDir,
            testsDir,
            projectName
        );
        CreateDirectoryBuildTargets(mainFolder);
        CreateDirectoryPackagesProps(mainFolder);
        const string directoryPackagesProps = "Directory.Packages.props";
        string directoryPackagePropsFilePath = _fileSystem.Path.Combine(
            mainFolder,
            directoryPackagesProps
        );
        var testProjectFilePath = _fileSystem.Path.Combine(
            testProjectPath,
            $"{testProjectName}.csproj"
        );
        MovePackageVersionsToDirectoryPackagesProps(
            testProjectFilePath,
            directoryPackagePropsFilePath
        );
        InitializeGitRepository(mainFolder);
        InstallDotnetTools(mainFolder);

        var huskyService = new HuskyService(_fileSystem, _commandService);
        var huskyHooksResult = huskyService.InitializeHuskyHooks(mainFolder);
        if (huskyHooksResult is not null)
        {
            return huskyHooksResult;
        }

        huskyService.InitializeHuskyRestoreTarget(mainFolder);

        return InitResult.Success(
            solutionName,
            projectName,
            mainFolder,
            selectedFolders,
            solutionOutputDir,
            testProjectPath
        );
    }

    private void CreateFolders(string mainFolder, List<string> folders)
    {
        _fileSystem.Directory.CreateDirectory(mainFolder);
        folders.ForEach(folder =>
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(mainFolder, folder))
        );
    }

    private void CreateProjectAndSolution(
        string solutionOutputDir,
        string solutionName,
        string projectName
    )
    {
        _commandService.RunCommand(
            "dotnet",
            $"new sln --name \"{solutionName}\" --output \"{solutionOutputDir}\""
        );
        _commandService.RunCommand(
            "dotnet",
            $"new console --output \"{projectName}\"",
            solutionOutputDir
        );
        _commandService.RunCommand("dotnet", $"sln add \"{projectName}\"", solutionOutputDir);
    }

    private (string testProjectName, string testProjectPath) CreateTestProject(
        string solutionOutputDir,
        string testsOutputDir,
        string projectName
    )
    {
        var testProjectName = $"{projectName}.Tests";
        var testProjectPath = _fileSystem.Path.Combine(testsOutputDir, testProjectName);
        testProjectPath = _fileSystem.Path.GetFullPath(testProjectPath);
        _commandService.RunCommand("dotnet", $"new nunit --name {testProjectName}", testsOutputDir);
        _commandService.RunCommand(
            "dotnet",
            $"sln add \"{testProjectPath}\" --solution-folder Tests",
            solutionOutputDir
        );

        return (testProjectName, testProjectPath);
    }

    private void MovePackageVersionsToDirectoryPackagesProps(
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

    private void CreateDirectoryBuildTargets(string mainFolder)
    {
        var directoryBuildTargetsFile = _fileSystem.Path.Combine(
            mainFolder,
            "Directory.Build.targets"
        );
        var doc = new XDocument(new XElement("Project"));
        _fileSystem.File.WriteAllText(directoryBuildTargetsFile, doc.ToString());
    }

    private void CreateDirectoryPackagesProps(string mainFolder)
    {
        var directoryPackagesPropsFile = _fileSystem.Path.Combine(
            mainFolder,
            "Directory.Packages.props"
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

    private void InstallDotnetTools(string mainFolder)
    {
        _commandService.RunCommand("dotnet", "new tool-manifest", mainFolder);
        _commandService.RunCommand("dotnet", "tool install csharpier", mainFolder);
        _commandService.RunCommand("dotnet", "tool install husky", mainFolder);
        _commandService.RunCommand("dotnet", "husky install", mainFolder);
    }

    private void InitializeGitRepository(string mainFolder)
    {
        _commandService.RunCommand("git", "init", mainFolder);
    }
}
