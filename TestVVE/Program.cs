using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TestVVE;

#region Build

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .ConfigureHttpJsonOptions(o =>
        o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddDbContext<TaskDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")))
    .AddSingleton<RabbitMqService>();

var app = builder.Build();

#endregion

#region Endpoints

app.MapPost("/tasks", async (CreateTaskRequest request, TaskDbContext context) =>
{
    var title = request.Title?.Trim() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
    {
        return Results.BadRequest("Incorrect title.");
    }

    var taskItem = new TaskItem
    {
        Id = Guid.NewGuid(),
        Title = title,
        Priority = request.Priority ?? Priority.Medium,
        CreatedAt = DateTimeOffset.UtcNow,
    };

    context.Add(taskItem);
    await context.SaveChangesAsync();

    return Results.Created($"/tasks/{taskItem.Id}", taskItem);
});

app.MapGet("/tasks", async (TaskDbContext context) => 
{
    var taskItems = await context.TaskItems
        .AsNoTracking()
        .ToListAsync();

    return Results.Ok(taskItems);
});

app.MapPut("/tasks/{id}/complete", async (Guid id, TaskDbContext context, RabbitMqService rabbitMqService) =>
{
    var taskItem = await context.TaskItems.FirstOrDefaultAsync(x => x.Id == id);
    if (taskItem is null)
        return Results.NotFound();
    if (taskItem.IsCompleted)
        return Results.Conflict("Task already completed.");

    taskItem.IsCompleted = true;
    taskItem.CompletedAt = DateTimeOffset.UtcNow;

    try
    {
        await context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(new { message = "Concurrency conflict." });
    }

    var taskItemMessage = new TaskItemMessage
    {
        TaskId = taskItem.Id,
        Title = taskItem.Title,
        CompletedAt = taskItem.CompletedAt,
        Priority = taskItem.Priority
    };
    await rabbitMqService.PublishAsync(taskItemMessage, "task.completed");

    return Results.Ok(taskItem);
});

app.MapDelete("/tasks/{id}", async (Guid id, TaskDbContext context) =>
{
    var taskItem = await context.TaskItems.FirstOrDefaultAsync(x => x.Id == id);
    if (taskItem is null)
        return Results.NotFound();

    context.TaskItems.Remove(taskItem);
    await context.SaveChangesAsync();

    return Results.NoContent();
});

#endregion


app.Lifetime.ApplicationStopping.Register(() =>
{
    var rabbitMqService = app.Services.GetRequiredService<RabbitMqService>();
    rabbitMqService.DisposeAsync().AsTask().Wait(TimeSpan.FromSeconds(5));
});

app.Run();

public partial class Program { }