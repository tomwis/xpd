using System.IO.Abstractions;
using System.Runtime.InteropServices;
using xpd.Services;

namespace xpd.Models;

internal class FileTemplate(IFileSystem fileSystem)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private IFileInfo? _outputFile;
    private IDictionary<string, string>? _tokenMap;
    private string? _templateName;

    public FileTemplate From(string templateName)
    {
        _templateName = templateName;
        return this;
    }

    public FileTemplate To(IFileInfo outputFile)
    {
        _outputFile = outputFile;
        return this;
    }

    public FileTemplate WithTokens(IDictionary<string, string> tokenMap)
    {
        _tokenMap = tokenMap;
        return this;
    }

    public void Save(bool asExecutable = false)
    {
        if (_templateName is null)
        {
            throw new NullReferenceException(
                $"Use {nameof(From)} method to set template name before calling {nameof(Save)} method."
            );
        }

        if (_outputFile is null)
        {
            throw new NullReferenceException(
                $"Use {nameof(To)} method to set output file before calling {nameof(Save)} method."
            );
        }

        var content = ResourceProvider.GetResource(_templateName);

        if (_tokenMap is not null)
        {
            foreach (var (key, value) in _tokenMap)
            {
                content = content.Replace($"{key}", value);
            }
        }

        _fileSystem.File.WriteAllText(_outputFile.FullName, content);

        if (asExecutable && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var fileMode = _fileSystem.File.GetUnixFileMode(_outputFile!.FullName);
            fileMode |=
                UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
            _fileSystem.File.SetUnixFileMode(_outputFile!.FullName, fileMode);
        }
    }
}
