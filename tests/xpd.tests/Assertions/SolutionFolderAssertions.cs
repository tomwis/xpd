using FluentAssertions;
using FluentAssertions.Primitives;
using xpd.SolutionModifier;

namespace xpd.tests.Assertions;

internal class SolutionFolderAssertions(SolutionFolder folder)
    : ReferenceTypeAssertions<SolutionFolder, SolutionFolderAssertions>(folder)
{
    protected override string Identifier => nameof(SolutionFolder);

    public AndWhichConstraint<SolutionFolderAssertions, SolutionItem> HaveItem(
        string name,
        string path
    )
    {
        var itemConstraint = Subject
            .Items.Should()
            .Contain(
                item => item.Name == name && item.Path == path,
                $"Expected SolutionItem with Name={name} and Path={path}."
            );

        return new AndWhichConstraint<SolutionFolderAssertions, SolutionItem>(
            this,
            itemConstraint.Which
        );
    }
}
