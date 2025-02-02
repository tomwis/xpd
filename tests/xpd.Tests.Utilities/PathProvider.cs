﻿using xpd.Constants;

namespace xpd.Tests.Utilities;

public static class PathProvider
{
    public static string GetRootRepoFolder()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir is not null && !HasGitFolder(currentDir))
        {
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        if (currentDir is null)
        {
            throw new Exception("Could not find root repo folder.");
        }

        // Make additional checks for common files to make sure it is root repo folder
        var packagesPropsExists = new FileInfo(
            Path.Combine(currentDir, FileConstants.DirectoryPackagesProps)
        ).Exists;
        var solutionExists = new DirectoryInfo(currentDir).GetFiles("*.sln").Length == 1;
        var solutionExistsInSrc =
            new DirectoryInfo(Path.Combine(currentDir, OptionalFoldersConstants.SrcDir))
                .GetFiles("*.sln")
                .Length == 1;

        if (!packagesPropsExists && !(solutionExists || solutionExistsInSrc))
        {
            throw new Exception("Root repo folder doesn't have required files.");
        }

        Console.WriteLine($"Root repo folder: {currentDir}");
        return currentDir;

        static bool HasGitFolder(string folder) =>
            Directory.EnumerateFileSystemEntries(folder).Any(f => f.EndsWith(".git"));
    }

    public static string PrepareOutputDirForIntegrationTests(string suffix)
    {
        const string outputDir = "XpdIntegrationTestsOutputDir";
        var rootRepoFolder = GetRootRepoFolder();
        var outputPath = Path.Combine(rootRepoFolder, "..", outputDir, $"Test-{suffix}");
        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }

        return outputPath;
    }
}
