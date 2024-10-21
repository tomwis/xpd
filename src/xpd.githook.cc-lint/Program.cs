using System.IO.Abstractions;
using CommandLine;

namespace xpd.githook.cc_lint;

public class Program
{
    public static void Main(string[] args)
    {
        Parser
            .Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                var fileSystem = new FileSystem();
                new Linter().Run(
                    fileSystem.FileInfo.New(o.CommitMessageFileName),
                    fileSystem.FileInfo.New(o.CommitMessageConfigFileName)
                );
            })
            .WithNotParsed(errors => { });
    }
}
