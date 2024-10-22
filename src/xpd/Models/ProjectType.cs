using System.Text.Json;
using xpd.Services;

namespace xpd.Models;

public class ProjectType
{
    private readonly string _type;

    private ProjectType(string type)
    {
        var projectTypesJson = ResourceProvider.GetResource("project_types.json");
        var allowedProjectTypes = JsonSerializer.Deserialize<List<string>>(projectTypesJson);

        if (allowedProjectTypes is null)
        {
            throw new InvalidOperationException(
                $"Couldn't parse list of supported project types. Raw value: {Environment.NewLine}{projectTypesJson}"
            );
        }

        if (!allowedProjectTypes.Contains(type))
        {
            throw new InvalidOperationException(
                $"Unsupported project type was passed into -p/--project-type ({type}). Supported values: {string.Join(", ", allowedProjectTypes)}"
            );
        }

        _type = type;
    }

    public override string ToString()
    {
        return _type;
    }

    public static implicit operator ProjectType(string type) => new(type);

    public static implicit operator string(ProjectType type) => type.ToString();
}
