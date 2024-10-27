using xpd.SolutionModifier;

namespace xpd.Tests.Assertions.Extensions;

internal static class SolutionFolderAssertionsExtensions
{
    public static SolutionFolderAssertions Should(this SolutionFolder solutionFolder)
    {
        return new SolutionFolderAssertions(solutionFolder);
    }
}
