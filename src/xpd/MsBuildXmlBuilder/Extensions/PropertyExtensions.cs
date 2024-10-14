namespace xpd.MsBuildXmlBuilder.Extensions;

internal static class PropertyExtensions
{
    public static string ToUnevaluatedValue<T>(this T property)
    {
        return $"$({property})";
    }
}
