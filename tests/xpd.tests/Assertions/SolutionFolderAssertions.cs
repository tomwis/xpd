using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using xpd.SolutionModifier;
using xpd.tests.Assertions.Models;

namespace xpd.tests.Assertions;

internal class SolutionFolderAssertions(SolutionFolder folder)
    : ReferenceTypeAssertions<SolutionFolder, SolutionFolderAssertions>(folder)
{
    protected override string Identifier => nameof(SolutionFolder);

    [CustomAssertion]
    public AndWhichConstraint<SolutionFolderAssertions, SolutionItem> HaveItem(
        string name,
        string path
    )
    {
        var solutionItem = Subject.Items.FirstOrDefault(item =>
            item.Name == name && item.Path == path
        );

        var failureMessage = FailureMessage.ForEnumerable(
            () => Subject.Items,
            item => item.ToString(),
            new SolutionItem(name, path).ToString()
        );

        Execute.Assertion.ForCondition(solutionItem is not null).FailWith(failureMessage);

        return new AndWhichConstraint<SolutionFolderAssertions, SolutionItem>(this, solutionItem!);
    }
}
