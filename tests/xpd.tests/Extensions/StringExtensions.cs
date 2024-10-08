using System.Text.Json;

namespace xpd.tests.Extensions;

public static class StringExtensions
{
    public static T Deserialize<T>(this string fileName)
    {
        var json = File.ReadAllText(fileName);
        return JsonSerializer.Deserialize<T>(json)!;
    }

    public static FileInfo ToFile(this string fileName)
    {
        return new FileInfo(fileName);
    }
}
