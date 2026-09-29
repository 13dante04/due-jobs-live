namespace DueJobs.Api;

// This service predates the interview tasks. Keep its existing routes usable while improving it.
public sealed class JobService(IJobRepository jobs, IClock clock, IDeliveryGateway delivery)
{
    public Job Create(string title, DateTimeOffset scheduledAt, JobKind kind = JobKind.Email, string target = "operator@example.test")
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
        if (scheduledAt <= clock.UtcNow) throw new ArgumentException("Scheduled time must be in the future.");
        var job = new Job(Guid.NewGuid(), title.Trim(), scheduledAt, JobStatus.Pending, Kind: kind, Target: target);
        jobs.Add(job);
        return job;
    }

    public Job? Cancel(Guid id)
    {
        var job = jobs.GetById(id);
        if (job is null) return null;
        if (job.Status == JobStatus.Pending) jobs.Update(job with { Status = JobStatus.Cancelled });
        return jobs.GetById(id);
    }

    public int ProcessDue()
    {
        var now = clock.UtcNow;
        var count = 0;
        foreach (var job in jobs.GetAll())
        {
            if (job.Status != JobStatus.Completed && job.ScheduledAt < now)
            {
                // The original feature only sends email. The next ticket adds another delivery type.
                delivery.SendEmail(job.Target, job.Title);
                jobs.Update(job with { Status = JobStatus.Completed, Attempts = job.Attempts + 1 });
                count++;
            }
        }
        return count;
    }
}
