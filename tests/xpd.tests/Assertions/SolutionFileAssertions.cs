using NUnit.Framework;
using xpd.SolutionModifier;
using xpd.tests.Assertions.Models;

namespace xpd.tests.Assertions;

using FluentAssertions;
using FluentAssertions.Primitives;

internal class SolutionFileAssertions(SolutionFileForTest solutionFile)
    : ReferenceTypeAssertions<SolutionFileForTest, SolutionFileAssertions>(solutionFile)
{
    protected override string Identifier => nameof(SolutionFileForTest);

    public AndWhichConstraint<SolutionFileAssertions, SolutionFolder> HaveSolutionFolder(
        string name
    )
    {
        var folder = Subject.SolutionFolders.FirstOrDefault(f => f.Name == name);
        Assert.That(folder, Is.Not.Null);
        return new AndWhichConstraint<SolutionFileAssertions, SolutionFolder>(this, folder);
    }
}
