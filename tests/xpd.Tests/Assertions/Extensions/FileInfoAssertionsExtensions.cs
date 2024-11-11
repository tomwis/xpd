using System.IO.Abstractions;

namespace xpd.Tests.Assertions.Extensions;

internal static class FileInfoAssertionsExtensions
{
    public static FileInfoAssertions Should(this IFileInfo fileInfo)
    {
        return new FileInfoAssertions(fileInfo);
    }
}
