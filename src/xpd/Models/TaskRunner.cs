using System.Text.Json.Serialization;

namespace xpd.Models;

public class TaskRunner
{
    [JsonPropertyName("$schema")]
    public string Schema { get; init; } = null!;

    [JsonPropertyName("tasks")]
    public List<TaskRunnerTask> Tasks { get; init; } = null!;
}
