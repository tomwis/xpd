using System.Linq.Expressions;
using System.Text;
using xpd.Tests.Assertions.Extensions;

namespace xpd.Tests.Assertions.Models;

public static class FailureMessage
{
    public static string ForEnumerable<TCollectionItem>(
        Expression<Func<IEnumerable<TCollectionItem>>> collectionSelector,
        Func<TCollectionItem, string> itemFormatter,
        string expectedItemFormatted
    )
    {
        var memberName = GetMemberName(collectionSelector);
        var parentName = GetParentName(collectionSelector);
        var collection = collectionSelector.Compile()();

        var builder = new StringBuilder("Didn't find expected item in collection ");
        builder.Append($"{parentName}.{memberName}");
        builder.Append(" of type ");
        builder.Append(GetReadableName(collection.GetType()));
        builder.AppendLine(".");
        builder.AppendLine();
        builder.AppendLine("Expected item:");
        builder.AppendLine(expectedItemFormatted);
        builder.AppendLine();
        builder.Append("Actual collection:");
        builder.AppendLine(collection.ToString(itemFormatter));
        return builder.ToString();
    }

    private static string? GetMemberName<TCollectionItem>(
        Expression<Func<IEnumerable<TCollectionItem>>> collectionSelector
    )
    {
        return collectionSelector.Body is not MemberExpression memberExpression
            ? null
            : memberExpression.Member.Name;
    }

    private static string? GetParentName(LambdaExpression expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
            return null;

        var parents = new List<string>();
        while (memberExpression.Expression is MemberExpression parentExpression)
        {
            var memberName =
                parentExpression.Expression is MemberExpression
                    ? parentExpression.Member.Name
                    : GetReadableName(parentExpression.Type);

            parents.Add(memberName);
            memberExpression = parentExpression;
        }

        return string.Join(".", parents);
    }

    private static string GetReadableName(this Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var genericTypeName = type.GetGenericTypeDefinition().Name;
        genericTypeName = genericTypeName[..genericTypeName.IndexOf('`')];
        var genericArgs = type.GetGenericArguments().Select(GetReadableName);
        var genericArguments = string.Join(", ", genericArgs);
        return $"{genericTypeName}<{genericArguments}>";
    }
}
