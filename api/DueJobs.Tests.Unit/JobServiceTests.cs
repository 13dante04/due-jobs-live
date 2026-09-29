using DueJobs.Api;

namespace DueJobs.Tests.Unit;

public sealed class JobServiceTests
{
    private readonly FixedClock _clock = new();
    private readonly InMemoryJobRepository _repository = new();
    private readonly RecordingDeliveryGateway _delivery = new();
    private JobService CreateService() => new(_repository, _clock, _delivery);

    [Fact]
    public void Create_TrimsTitleAndStoresPendingJob()
    {
        var scheduledAt = _clock.UtcNow.AddHours(1);
        var job = CreateService().Create("  Weekly report  ", scheduledAt);

        Assert.NotEqual(Guid.Empty, job.Id);
        Assert.Equal("Weekly report", job.Title);
        Assert.Equal(scheduledAt, job.ScheduledAt);
        Assert.Equal(JobStatus.Pending, job.Status);
        Assert.Equal(job, Assert.Single(_repository.GetAll()));
        Assert.Empty(_delivery.Deliveries);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsBlankTitle(string title)
    {
        Assert.Throws<ArgumentException>(() => CreateService().Create(title, _clock.UtcNow.AddHours(1)));
        Assert.Empty(_repository.GetAll());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Create_RejectsTimeThatIsNotInTheFuture(int offsetSeconds)
    {
        Assert.Throws<ArgumentException>(() => CreateService().Create("Report", _clock.UtcNow.AddSeconds(offsetSeconds)));
        Assert.Empty(_repository.GetAll());
    }

    [Fact]
    public void Cancel_PendingJob_StoresCancelledStatusWithoutDelivery()
    {
        var service = CreateService();
        var job = service.Create("Report", _clock.UtcNow.AddHours(1));

        var cancelled = service.Cancel(job.Id);

        Assert.NotNull(cancelled);
        Assert.Equal(JobStatus.Cancelled, cancelled.Status);
        Assert.Equal(cancelled, _repository.GetById(job.Id));
        Assert.Empty(_delivery.Deliveries);
    }

    [Fact]
    public void Cancel_UnknownJob_ReturnsNull() => Assert.Null(CreateService().Cancel(Guid.NewGuid()));

    [Fact]
    public void ProcessDue_DeliversDueEmailAndDoesNotRepeatCompletedDelivery()
    {
        var job = new Job(Guid.NewGuid(), "Report", _clock.UtcNow.AddMinutes(-1), JobStatus.Pending);
        _repository.Add(job);
        var service = CreateService();

        Assert.Equal(1, service.ProcessDue());
        Assert.Equal(0, service.ProcessDue());

        Assert.Equal($"Email:{job.Target}:{job.Title}", Assert.Single(_delivery.Deliveries));
        var stored = _repository.GetById(job.Id);
        Assert.NotNull(stored);
        Assert.Equal(JobStatus.Completed, stored.Status);
        Assert.Equal(1, stored.Attempts);
    }

    [Fact]
    public void ProcessDue_LeavesFutureJobPending()
    {
        var service = CreateService();
        var job = service.Create("Later", _clock.UtcNow.AddHours(1));

        Assert.Equal(0, service.ProcessDue());
        Assert.Equal(job, _repository.GetById(job.Id));
        Assert.Empty(_delivery.Deliveries);
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2030, 1, 1, 12, 0, 0, TimeSpan.Zero);
    }
}
