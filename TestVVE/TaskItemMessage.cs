namespace TestVVE;

public class TaskItemMessage
{
    public Guid TaskId { get; set; }

    public string Title { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public Priority Priority { get; set; }
}
