using FluentAssertions;
using NUnit.Framework;
using xpd.Tests.Utilities;

namespace xpd.ConventionTests;

[TestFixture]
public class EnvFileSyncTests
{
    private const string EnvFilesDirectory = "config/";
    private static readonly string[] EnvFiles = { ".env", ".env.example" };

    [Test]
    public void EnvFilesShouldHaveSameKeys()
    {
        var envFiles = GetEnvFiles();
        var keys = File.ReadAllLines(envFiles[0])
            .Select(line => line.Split('=')[0].Trim())
            .ToList();

        foreach (var file in envFiles.Skip(1))
        {
            var fileKeys = File.ReadAllLines(file).Select(line => line.Split('=')[0].Trim());
            fileKeys.Should().BeEquivalentTo(keys);
        }
    }

    [Test]
    public void EnvFilesShouldHaveSameKeyOrder()
    {
        var envFiles = GetEnvFiles();
        var keys = File.ReadAllLines(envFiles[0])
            .Select(line => line.Split('=')[0].Trim())
            .ToList();

        foreach (var file in envFiles.Skip(1))
        {
            var fileKeys = File.ReadAllLines(file).Select(line => line.Split('=')[0].Trim());
            fileKeys.Should().Equal(keys);
        }
    }

    private static string[] GetEnvFiles()
    {
        var rootRepoFolder = PathProvider.GetRootRepoFolder();
        var configDir = Path.Combine(rootRepoFolder, EnvFilesDirectory);
        var envFiles = Directory.GetFiles(configDir, "*.env*");
        Console.WriteLine($"Got env files: {string.Join(", ", envFiles)}");
        return envFiles;
    }
}
