using System.IO.Abstractions;
using NUnit.Framework;

namespace xpd.CommitLinter.Tests;

public class LinterTests
{
    [Test]
    public void CommitMessageWithCorrectSubject()
    {
        // Arrange
        var subject = GetSubject();
        var fileSystem = new FileSystem();
        var commitFile = fileSystem.FileInfo.New("TestFiles/commit.txt");
        var commitConfigFile = fileSystem.FileInfo.New("TestFiles/commit-message-config.json");

        // Act
        subject.Run(commitFile, commitConfigFile);
    }

    private static Linter GetSubject()
    {
        return new Linter();
    }
}
