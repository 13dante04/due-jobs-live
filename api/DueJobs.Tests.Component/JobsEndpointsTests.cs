using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DueJobs.Api;
using DueJobs.Tests.Component.Infrastructure;

namespace DueJobs.Tests.Component;

// xUnit creates a new test class (and therefore a fresh host/repository) per test.
public sealed class JobsEndpointsTests : IDisposable
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly JobsApiFactory _factory = new();
    private readonly HttpClient _client;

    public JobsEndpointsTests() => _client = _factory.CreateClient();

    [Fact]
    public async Task GetJobs_NewHost_ReturnsEmptyArray()
    {
        var jobs = await _client.GetFromJsonAsync<Job[]>("/jobs", Json);
        Assert.NotNull(jobs);
        Assert.Empty(jobs);
    }

    [Fact]
    public async Task CreateJob_ReturnsCreatedLocationAndStringStatus_AndCanBeListed()
    {
        using var response = await _client.PostAsJsonAsync("/jobs", new
        {
            title = "  Report  ", scheduledAt = _factory.Clock.UtcNow.AddHours(1)
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Pending", body.GetProperty("status").GetString());
        var job = body.Deserialize<Job>(Json)!;
        Assert.Equal($"/jobs/{job.Id}", response.Headers.Location?.OriginalString);
        Assert.Equal("Report", job.Title);
        var jobs = await _client.GetFromJsonAsync<Job[]>("/jobs", Json);
        Assert.Equal(job, Assert.Single(jobs!));
    }

    [Theory]
    [InlineData("", 60, "Title is required.")]
    [InlineData("Report", 0, "Scheduled time must be in the future.")]
    public async Task CreateJob_InvalidRequest_ReturnsBadRequestWithoutSaving(string title, int offsetSeconds, string error)
    {
        using var response = await _client.PostAsJsonAsync("/jobs", new
        {
            title, scheduledAt = _factory.Clock.UtcNow.AddSeconds(offsetSeconds)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(error, body.GetProperty("error").GetString());
        Assert.Empty((await _client.GetFromJsonAsync<Job[]>("/jobs", Json))!);
    }

    [Fact]
    public async Task CancelJob_UpdatesStatusReturnedByList()
    {
        var job = await CreateJob("Cancel me", _factory.Clock.UtcNow.AddHours(1));

        using var response = await _client.PostAsync($"/jobs/{job.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cancelled = await response.Content.ReadFromJsonAsync<Job>(Json);
        Assert.NotNull(cancelled);
        Assert.Equal(JobStatus.Cancelled, cancelled.Status);
        Assert.Equal(cancelled, Assert.Single((await _client.GetFromJsonAsync<Job[]>("/jobs", Json))!));
        Assert.Empty(_factory.Delivery.Deliveries);
    }

    [Fact]
    public async Task CancelJob_UnknownId_ReturnsNotFound()
    {
        using var response = await _client.PostAsync($"/jobs/{Guid.NewGuid()}/cancel", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetJobs_OrdersByScheduledTime()
    {
        var later = await CreateJob("Later", _factory.Clock.UtcNow.AddHours(2));
        var earlier = await CreateJob("Earlier", _factory.Clock.UtcNow.AddHours(1));

        var jobs = await _client.GetFromJsonAsync<Job[]>("/jobs", Json);

        Assert.Equal(new[] { earlier.Id, later.Id }, jobs!.Select(job => job.Id));
    }

    [Fact]
    public async Task ProcessDue_CompletesDueEmailThroughHttpWorkflow()
    {
        var job = await CreateJob("Report", _factory.Clock.UtcNow.AddMinutes(1));
        _factory.Clock.UtcNow = _factory.Clock.UtcNow.AddMinutes(2);

        using var response = await _client.PostAsync("/jobs/process-due", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, result.GetProperty("processed").GetInt32());
        var stored = Assert.Single((await _client.GetFromJsonAsync<Job[]>("/jobs", Json))!);
        Assert.Equal(job.Id, stored.Id);
        Assert.Equal(JobStatus.Completed, stored.Status);
        Assert.Equal(1, stored.Attempts);
        Assert.Equal($"Email:{job.Target}:{job.Title}", Assert.Single(_factory.Delivery.Deliveries));
    }

    private async Task<Job> CreateJob(string title, DateTimeOffset scheduledAt)
    {
        using var response = await _client.PostAsJsonAsync("/jobs", new { title, scheduledAt });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<Job>(Json))!;
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
