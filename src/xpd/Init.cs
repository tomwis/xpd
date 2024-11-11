using CommandLine;

namespace xpd;

[Verb("init")]
public class Init
{
    [Option('o', "output", Required = false, HelpText = "Parent folder for solution.")]
    public string? Output { get; set; }

    [Value(0, Required = false, HelpText = "Solution name. Use like: init \"MySolutionName\"")]
    public string? SolutionName { get; set; }

    [Option(
        'p',
        "project-type",
        Required = false,
        HelpText = "Project type to be created. Supported values: console, classlib, maui, mauilib, ios, ios-tabbed, ioslib, iosbinding, android, androidlib, android-bindinglib"
    )]
    public string? ProjectType { get; set; }
}
