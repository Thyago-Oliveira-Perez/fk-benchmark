using BenchmarkDotNet.Attributes;
using Dapper;
using Npgsql;

namespace DeleteBenchmarks;

[MemoryDiagnoser]
[IterationCount(20)]
public class DeleteBench
{
    private const string ConnectionStringWithFk = "Host=localhost;Port=5432;Database=benchmark_with_fk;Username=benchmark;Password=benchmark123";
    private const string ConnectionStringWithoutFk = "Host=localhost;Port=5433;Database=benchmark_without_fk;Username=benchmark;Password=benchmark123";

    private const int SmallVolumeTransactions = 100;
    private const int MediumVolumeTransactions = 1000;
    private const int LargeVolumeTransactions = 10000;

    private long _tempUserId = 200000;

    [GlobalSetup]
    public async Task Setup()
    {
        Console.WriteLine("Setting up CASCADE benchmarks with REAL commits...");
    }

    private async Task<long> CreateUserWithTransactions(string connectionString, int transactionCount)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        var userId = Interlocked.Increment(ref _tempUserId);

        await conn.ExecuteAsync(@"
            INSERT INTO users (id, email, name) 
            VALUES (@UserId, @Email, @Name)
        ", new { UserId = userId, Email = $"temp{userId}@test.com", Name = $"Temp User {userId}" });

        await conn.ExecuteAsync($@"
            INSERT INTO transactions (user_id, account_id, merchant_id, category_id, amount, description, transaction_type, status)
            SELECT 
                @UserId,
                (SELECT id FROM accounts ORDER BY random() LIMIT 1),
                (SELECT id FROM merchants ORDER BY random() LIMIT 1),
                (SELECT id FROM categories ORDER BY random() LIMIT 1),
                (random() * 500 + 10)::decimal(18,2),
                'Benchmark txn ' || generate_series,
                'PURCHASE',
                'COMPLETED'
            FROM generate_series(1, @Count)
        ", new { UserId = userId, Count = transactionCount });

        return userId;
    }

    [Benchmark(Description = "CASCADE Delete (100 txns) - WITH FK")]
    public async Task CascadeDelete_Small_WithFK()
    {
        var userId = await CreateUserWithTransactions(ConnectionStringWithFk, SmallVolumeTransactions);
        
        await using var conn = new NpgsqlConnection(ConnectionStringWithFk);
        await conn.OpenAsync();
        await conn.ExecuteAsync("DELETE FROM users WHERE id = @UserId", new { UserId = userId });
    }

    [Benchmark(Description = "Manual Delete (100 txns) - WITHOUT FK")]
    public async Task ManualDelete_Small_WithoutFK()
    {
        var userId = await CreateUserWithTransactions(ConnectionStringWithoutFk, SmallVolumeTransactions);
        
        await using var conn = new NpgsqlConnection(ConnectionStringWithoutFk);
        await conn.OpenAsync();
        await conn.ExecuteAsync("DELETE FROM transactions WHERE user_id = @UserId", new { UserId = userId });
        await conn.ExecuteAsync("DELETE FROM users WHERE id = @UserId", new { UserId = userId });
    }

    [Benchmark(Description = "CASCADE Delete (1K txns) - WITH FK")]
    public async Task CascadeDelete_Medium_WithFK()
    {
        var userId = await CreateUserWithTransactions(ConnectionStringWithFk, MediumVolumeTransactions);
        
        await using var conn = new NpgsqlConnection(ConnectionStringWithFk);
        await conn.OpenAsync();
        await conn.ExecuteAsync("DELETE FROM users WHERE id = @UserId", new { UserId = userId });
    }

    [Benchmark(Description = "Manual Delete (1K txns) - WITHOUT FK")]
    public async Task ManualDelete_Medium_WithoutFK()
    {
        var userId = await CreateUserWithTransactions(ConnectionStringWithoutFk, MediumVolumeTransactions);
        
        await using var conn = new NpgsqlConnection(ConnectionStringWithoutFk);
        await conn.OpenAsync();
        await conn.ExecuteAsync("DELETE FROM transactions WHERE user_id = @UserId", new { UserId = userId });
        await conn.ExecuteAsync("DELETE FROM users WHERE id = @UserId", new { UserId = userId });
    }

    [Benchmark(Description = "CASCADE Delete (10K txns) - WITH FK")]
    public async Task CascadeDelete_Large_WithFK()
    {
        var userId = await CreateUserWithTransactions(ConnectionStringWithFk, LargeVolumeTransactions);
        
        await using var conn = new NpgsqlConnection(ConnectionStringWithFk);
        await conn.OpenAsync();
        await conn.ExecuteAsync("DELETE FROM users WHERE id = @UserId", new { UserId = userId });
    }

    [Benchmark(Description = "Manual Delete (10K txns) - WITHOUT FK")]
    public async Task ManualDelete_Large_WithoutFK()
    {
        var userId = await CreateUserWithTransactions(ConnectionStringWithoutFk, LargeVolumeTransactions);
        
        await using var conn = new NpgsqlConnection(ConnectionStringWithoutFk);
        await conn.OpenAsync();
        await conn.ExecuteAsync("DELETE FROM transactions WHERE user_id = @UserId", new { UserId = userId });
        await conn.ExecuteAsync("DELETE FROM users WHERE id = @UserId", new { UserId = userId });
    }

    // Soft Delete - just update existing user (no need to create 10K transactions)
    [Benchmark(Description = "Soft Delete (no CASCADE) - WITH FK")]
    public async Task SoftDelete_WithFK()
    {
        await using var conn = new NpgsqlConnection(ConnectionStringWithFk);
        await conn.OpenAsync();

        // Just update an existing user (soft delete is instant regardless of transaction count)
        await conn.ExecuteAsync("UPDATE users SET deleted_at = NOW() WHERE id = 50000 AND deleted_at IS NULL");
        // Reset for next iteration
        await conn.ExecuteAsync("UPDATE users SET deleted_at = NULL WHERE id = 50000");
    }

    [Benchmark(Description = "Soft Delete (no CASCADE) - WITHOUT FK")]
    public async Task SoftDelete_WithoutFK()
    {
        await using var conn = new NpgsqlConnection(ConnectionStringWithoutFk);
        await conn.OpenAsync();

        await conn.ExecuteAsync("UPDATE users SET deleted_at = NOW() WHERE id = 50000 AND deleted_at IS NULL");
        await conn.ExecuteAsync("UPDATE users SET deleted_at = NULL WHERE id = 50000");
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        Console.WriteLine("Cleaning up temporary test data...");
        
        await using var connWithFk = new NpgsqlConnection(ConnectionStringWithFk);
        await connWithFk.OpenAsync();
        await connWithFk.ExecuteAsync("DELETE FROM users WHERE id >= 200000");

        await using var connWithoutFk = new NpgsqlConnection(ConnectionStringWithoutFk);
        await connWithoutFk.OpenAsync();
        await connWithoutFk.ExecuteAsync("DELETE FROM transactions WHERE user_id >= 200000");
        await connWithoutFk.ExecuteAsync("DELETE FROM users WHERE id >= 200000");

        Console.WriteLine("Cleanup complete!");
    }
}
