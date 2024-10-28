using System.Text;

namespace xpd.Tests.Assertions.Extensions;

// ReSharper disable once InconsistentNaming
public static class IEnumerableExtensions
{
    public static string ToString<T>(this IEnumerable<T> items, Func<T, string> itemFormatter)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("[");
        var list = items.ToList();
        for (int i = 0; i < list.Count; i++)
        {
            stringBuilder.Append('\t');
            stringBuilder.Append(itemFormatter(list[i]));

            if (i < list.Count - 1)
            {
                stringBuilder.Append(',');
            }

            stringBuilder.AppendLine();
        }
        stringBuilder.AppendLine("]");
        return stringBuilder.ToString();
    }
}
