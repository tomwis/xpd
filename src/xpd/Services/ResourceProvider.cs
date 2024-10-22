namespace xpd.Services;

public static class ResourceProvider
{
    public static string GetResource(string resourceName)
    {
        var assembly = typeof(InitHandler).Assembly;
        var readmeResourceName = assembly
            .GetManifestResourceNames()
            .First(name => name.EndsWith(resourceName));
        using var stream = assembly.GetManifestResourceStream(readmeResourceName)!;
        var content = new StreamReader(stream).ReadToEnd();
        return content;
    }
}
