namespace DueJobs.Api;

public interface IJobRepository
{
    IReadOnlyList<Job> GetAll();
    Job? GetById(Guid id);
    void Add(Job job);
    void Update(Job job);
}

public sealed class InMemoryJobRepository : IJobRepository
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, Job> _jobs = new();

    public IReadOnlyList<Job> GetAll()
    {
        lock (_gate) return _jobs.Values.ToArray();
    }

    public Job? GetById(Guid id)
    {
        lock (_gate) return _jobs.GetValueOrDefault(id);
    }

    public void Add(Job job)
    {
        lock (_gate) _jobs.Add(job.Id, job);
    }

    public void Update(Job job)
    {
        lock (_gate) _jobs[job.Id] = job;
    }
}
