using System.IO.Abstractions.TestingHelpers;

namespace xpd.Tests.Extensions;

public static class MockFileSystemExtensions
{
    public static MockFileSystem WithExtensions(this MockFileSystem mockFileSystem)
    {
        StringExtensions.Set(mockFileSystem);
        FileInfoExtensions.Set(mockFileSystem);
        return mockFileSystem;
    }
}
