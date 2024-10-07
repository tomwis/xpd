using CommandLine;

namespace xpd;

public class Program
{
    public static void Main(string[] args)
    {
        var result = Parser.Default.ParseArguments(args, typeof(Init));
        var exitCode = result.MapResult((Init opts) => new Init().Parse(opts), errs => 0);
    }
}
