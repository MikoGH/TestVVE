using System.Text.Json.Serialization;

namespace TestVVE;

public class TaskItemMessage
{
    public Guid TaskId { get; set; }

    public string Title { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Priority Priority { get; set; }
}
