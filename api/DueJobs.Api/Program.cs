using FluentValidation;
using DueJobs.Api;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();
builder.Services.AddSingleton<IDeliveryGateway, RecordingDeliveryGateway>();
builder.Services.AddSingleton<IValidator<CreateJobRequest>, CreateJobValidator>();
builder.Services.AddSingleton<JobService>();

var app = builder.Build();

if (app.Configuration.GetValue("SeedExampleJobs", true))
{
    var repository = app.Services.GetRequiredService<IJobRepository>();
    var clock = app.Services.GetRequiredService<IClock>();
    repository.Add(new Job(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Send report", clock.UtcNow.AddHours(-2), JobStatus.Pending));
    repository.Add(new Job(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Review metrics", clock.UtcNow.AddHours(3), JobStatus.Pending));
    repository.Add(new Job(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Deliver export", clock.UtcNow.AddHours(-1), JobStatus.Failed, 1, "Upstream timeout", JobKind.Webhook, "https://example.test/export"));
}

app.MapControllers();

app.Run();

// Expose the entry point to the in-memory component test host.
public partial class Program { }
