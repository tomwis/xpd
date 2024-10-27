using xpd.Tests.Assertions.Models;

namespace xpd.Tests.Assertions.Extensions;

internal static class SolutionFileAssertionsExtensions
{
    public static SolutionFileAssertions Should(this SolutionFileForTest solutionFile)
    {
        return new SolutionFileAssertions(solutionFile);
    }
}
