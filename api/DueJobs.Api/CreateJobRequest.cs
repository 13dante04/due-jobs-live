namespace DueJobs.Api;

public sealed record CreateJobRequest(string Title, DateTimeOffset ScheduledAt, JobKind Kind = JobKind.Email, string Target = "operator@example.test");
