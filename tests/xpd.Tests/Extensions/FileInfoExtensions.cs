namespace xpd.Tests.Extensions;

public static class FileInfoExtensions
{
    public static string ReadAllText(this FileInfo fileInfo)
    {
        return File.ReadAllText(fileInfo.FullName);
    }

    public static string[] ReadAllLines(this FileInfo fileInfo)
    {
        return File.ReadAllLines(fileInfo.FullName);
    }
}
