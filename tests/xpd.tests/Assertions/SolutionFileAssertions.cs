using FluentAssertions.Execution;
using xpd.SolutionModifier;
using xpd.tests.Assertions.Models;

namespace xpd.tests.Assertions;

using FluentAssertions;
using FluentAssertions.Primitives;

internal class SolutionFileAssertions(SolutionFileForTest solutionFile)
    : ReferenceTypeAssertions<SolutionFileForTest, SolutionFileAssertions>(solutionFile)
{
    protected override string Identifier => nameof(SolutionFileForTest);

    [CustomAssertion]
    public AndWhichConstraint<SolutionFileAssertions, SolutionFolder> HaveSolutionFolder(
        string name
    )
    {
        var solutionFolder = Subject.SolutionFolders.FirstOrDefault(item => item.Name == name);
        var failureMessage = FailureMessage.ForEnumerable(
            () => Subject.SolutionFolders,
            item => item.ToString(),
            new SolutionFolder(name).ToString()
        );

        Execute.Assertion.ForCondition(solutionFolder is not null).FailWith(failureMessage);

        return new AndWhichConstraint<SolutionFileAssertions, SolutionFolder>(
            this,
            solutionFolder!
        );
    }
}
