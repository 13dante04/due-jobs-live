namespace DueJobs.Api;

public enum JobStatus { Pending, Completed, Cancelled, Failed }
public enum JobKind { Email, Webhook }

public sealed record Job(Guid Id, string Title, DateTimeOffset ScheduledAt, JobStatus Status, int Attempts = 0, string? LastError = null, JobKind Kind = JobKind.Email, string Target = "operator@example.test");
