using System.Text.Json.Serialization;

namespace xpd.Models;

public class TaskRunner
{
    [JsonPropertyName("tasks")]
    public List<TaskRunnerTask> Tasks { get; init; } = null!;
}
