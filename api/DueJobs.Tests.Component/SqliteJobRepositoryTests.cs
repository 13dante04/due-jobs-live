using DueJobs.Api;

namespace DueJobs.Tests.Component;

public sealed class SqliteJobRepositoryTests
{
    [Fact]
    public void ReopeningDatabase_PreservesCreatedAndUpdatedJobs()
    {
        var path = Path.Combine(Path.GetTempPath(), $"due-jobs-{Guid.NewGuid()}.db");
        var connectionString = $"Data Source={path};Pooling=False";
        try
        {
            var repository = new SqliteJobRepository(connectionString);
            var job = new Job(Guid.NewGuid(), "Report's title", DateTimeOffset.UtcNow,
                JobStatus.Pending, Kind: JobKind.Webhook, Target: "https://example.test/export");
            Assert.Null(repository.GetById(job.Id));
            repository.Add(job);

            var reopened = new SqliteJobRepository(connectionString);
            Assert.Equal(job, reopened.GetById(job.Id));
            var updated = job with { Status = JobStatus.Failed, Attempts = 1, LastError = "Timeout" };
            reopened.Update(updated);

            var reopenedAgain = new SqliteJobRepository(connectionString);
            Assert.Equal(updated, Assert.Single(reopenedAgain.GetAll()));
        }
        finally { File.Delete(path); }
    }
}
