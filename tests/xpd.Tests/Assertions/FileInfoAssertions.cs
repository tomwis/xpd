using System.IO.Abstractions;
using System.Runtime.Versioning;
using FluentAssertions.Execution;

namespace xpd.Tests.Assertions;

using FluentAssertions;
using FluentAssertions.Primitives;

internal class FileInfoAssertions(IFileInfo fileInfo)
    : ReferenceTypeAssertions<IFileInfo, FileInfoAssertions>(fileInfo)
{
    protected override string Identifier => nameof(IFileInfo);

    [CustomAssertion]
    [UnsupportedOSPlatform("windows")]
    public AndWhichConstraint<FileInfoAssertions, IFileInfo> BeExecutable()
    {
        var fileMode = Subject.FileSystem.File.GetUnixFileMode(Subject.FullName);
        Execute
            .Assertion.ForCondition(
                fileMode.HasFlag(UnixFileMode.UserExecute)
                    && fileMode.HasFlag(UnixFileMode.GroupExecute)
                    && fileMode.HasFlag(UnixFileMode.OtherExecute)
            )
            .FailWith("File is not executable for every user");

        return new AndWhichConstraint<FileInfoAssertions, IFileInfo>(this, Subject);
    }
}
