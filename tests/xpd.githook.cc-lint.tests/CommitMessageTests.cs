using NUnit.Framework;

namespace xpd.githook.cc_lint.tests;

public class CommitMessageTests
{
    [Ignore("not yet implemented correctly")]
    [Test]
    public void Test()
    {
        Program.Main(
            new[] { "-c", "TestFiles/commit.txt", "-o", "TestFiles/conventionalcommit.json" }
        );
    }
}
