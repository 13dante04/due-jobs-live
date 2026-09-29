using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace DueJobs.Api;

// Demo schema: the ID is indexed; the remaining job fields are stored as JSON.
// Each operation owns its connection, so the repository can be a singleton.
public sealed class SqliteJobRepository : IJobRepository
{
    private readonly string _connectionString;

    public SqliteJobRepository(string connectionString)
    {
        _connectionString = connectionString;
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS Jobs (Id TEXT PRIMARY KEY, Payload TEXT NOT NULL)";
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<Job> GetAll()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Payload FROM Jobs";
        using var reader = command.ExecuteReader();
        var jobs = new List<Job>();
        while (reader.Read()) jobs.Add(JsonSerializer.Deserialize<Job>(reader.GetString(0))!);
        return jobs;
    }

    public Job? GetById(Guid id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Payload FROM Jobs WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id.ToString());
        return command.ExecuteScalar() is string json ? JsonSerializer.Deserialize<Job>(json) : null;
    }

    public void Add(Job job) => Save(job, "INSERT INTO Jobs (Id, Payload) VALUES ($id, $payload)");

    // Match the in-memory repository's upsert behavior.
    public void Update(Job job) => Save(job,
        "INSERT INTO Jobs (Id, Payload) VALUES ($id, $payload) ON CONFLICT(Id) DO UPDATE SET Payload = excluded.Payload");

    private void Save(Job job, string sql)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", job.Id.ToString());
        command.Parameters.AddWithValue("$payload", JsonSerializer.Serialize(job));
        command.ExecuteNonQuery();
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}
