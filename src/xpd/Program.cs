using CommandLine;

namespace xpd;

public class Program
{
    public static void Main(string[] args)
    {
        var result = Parser.Default.ParseArguments(args, typeof(Init));
        var exitCode = result.MapResult(
            (Init opts) =>
            {
                var initResult = new InitHandler().Parse(opts);
                return (int)(initResult.Error ?? 0);
            },
            errs => 0
        );
    }
}
