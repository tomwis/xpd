using xpd.tests.Assertions.Models;

namespace xpd.tests.Assertions.Extensions;

internal static class SolutionFileAssertionsExtensions
{
    public static SolutionFileAssertions Should(this SolutionFileForTest solutionFile)
    {
        return new SolutionFileAssertions(solutionFile);
    }
}
