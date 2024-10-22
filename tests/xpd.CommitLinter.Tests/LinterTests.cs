using NUnit.Framework;

namespace xpd.CommitLinter.Tests;

public class LinterTests
{
    [Test]
    public void CommitMessageWithCorrectSubject()
    {
        // Arrange
        var subject = GetSubject();
        var linterConfig = new LinterConfig(
            "TestFiles/commit.txt",
            "TestFiles/commit-message-config.json"
        );

        // Act
        subject.Run(linterConfig);
    }

    private static Linter GetSubject()
    {
        return new Linter();
    }
}
