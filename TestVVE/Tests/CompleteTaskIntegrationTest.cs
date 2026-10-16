using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace TestVVE.Tests;

public class CompleteTaskIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CompleteTaskIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task CreateAndCompleteTask()
    {
        // Arrange
        var config = _factory.Services.GetRequiredService<IConfiguration>();
        var client = _factory.CreateClient();

        // Act Create
        var createResponse = await client.PostAsJsonAsync("/tasks", new
        {
            title = "Test",
            priority = 2
        }, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskItem>(JsonOptions);
        // Assert Create
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(created);
        Assert.False(created!.IsCompleted);
        Assert.NotEqual(created.Id, Guid.Empty);

        // Act Read
        var readResponse = await client.GetAsync("/tasks");
        var items = await readResponse.Content.ReadFromJsonAsync<List<TaskItem>>(JsonOptions);
        // Assert Read
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
        Assert.NotNull(items);
        Assert.NotEmpty(items);

        // Act Complete
        var completeResponse = await client.PutAsync($"/tasks/{created.Id}/complete", null);
        // Assert Complete
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        // Act Delete
        var deleteResponse = await client.DeleteAsync($"/tasks/{created.Id}");
        // Assert Delete
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
