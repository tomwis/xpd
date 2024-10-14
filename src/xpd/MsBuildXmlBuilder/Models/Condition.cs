namespace xpd.MsBuildXmlBuilder.Models;

public sealed class Condition
{
    private readonly string _expression;

    private Condition(string expression)
    {
        _expression = expression;
    }

    public static Condition Equals(string left, string right)
    {
        return new Condition($"'{left}' == '{right}'");
    }

    public static Condition NotEquals(string left, string right)
    {
        return new Condition($"'{left}' != '{right}'");
    }

    public static Condition GreaterThan(string left, string right)
    {
        return new Condition($"'{left}' > '{right}'");
    }

    public static Condition GreaterThanOrEqual(string left, string right)
    {
        return new Condition($"'{left}' >= '{right}'");
    }

    public static Condition LessThan(string left, string right)
    {
        return new Condition($"'{left}' < '{right}'");
    }

    public static Condition LessThanOrEqual(string left, string right)
    {
        return new Condition($"'{left}' <= '{right}'");
    }

    public static Condition And(Condition left, Condition right)
    {
        return new Condition($"{left._expression} AND {right._expression}");
    }

    public static Condition Or(Condition left, Condition right)
    {
        return new Condition($"{left._expression} OR {right._expression}");
    }

    public static Condition Not(Condition condition)
    {
        return new Condition($"!({condition._expression})");
    }

    public static Condition PropertyExists(string propertyName)
    {
        return new Condition($"Exists('$({propertyName})')");
    }

    public static Condition FileExists(string filePath)
    {
        return new Condition($"Exists('{filePath}')");
    }

    public static Condition HasValue(string propertyName)
    {
        return new Condition($"'{propertyName}' != ''");
    }

    public override string ToString()
    {
        return _expression;
    }
}
