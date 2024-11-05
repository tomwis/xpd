using System.IO.Abstractions;

namespace xpd.Tests.Extensions;

public static class FileInfoExtensions
{
    private static IFileSystem _fileSystem = null!;

    public static void Set(IFileSystem fileSystem) => _fileSystem = fileSystem;

    public static string ReadAllText(this IFileInfo fileInfo)
    {
        return _fileSystem.File.ReadAllText(fileInfo.FullName);
    }

    public static string[] ReadAllLines(this IFileInfo fileInfo)
    {
        return _fileSystem.File.ReadAllLines(fileInfo.FullName);
    }
}
