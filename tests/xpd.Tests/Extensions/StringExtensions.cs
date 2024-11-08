using System.IO.Abstractions;
using System.Text.Json;

namespace xpd.Tests.Extensions;

public static class StringExtensions
{
    private static IFileSystem _fileSystem = null!;

    public static void Set(IFileSystem fileSystem) => _fileSystem = fileSystem;

    public static T Deserialize<T>(this string fileName)
    {
        var json = File.ReadAllText(fileName);
        return JsonSerializer.Deserialize<T>(json)!;
    }

    public static IFileInfo ToFile(this string fileName)
    {
        if (_fileSystem is null)
            throw new NullReferenceException(
                $"Field {nameof(_fileSystem)} must be set with {nameof(Set)} method before using this extension."
            );

        return _fileSystem.FileInfo.New(fileName);
    }
}
