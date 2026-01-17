using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using Npgsql;
using System.Data;

namespace FkBenchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class TransactionBenchmarks
{
    private const string ConnStringWithFk = "Host=localhost;Port=5432;Database=benchmark_with_fk;Username=benchmark;Password=benchmark123";
    private const string ConnStringWithoutFk = "Host=localhost;Port=5433;Database=benchmark_without_fk;Username=benchmark;Password=benchmark123";
    
    private readonly Random _random = new Random(42);
    
    [GlobalSetup]
    public async Task Setup()
    {
        Console.WriteLine("Setting up databases...");
        await Task.Delay(1000);
        Console.WriteLine("Databases ready!");
    }

    [Benchmark(Description = "Sequential Insert WITH FK")]
    public async Task SequentialInsert_WithFK()
    {
        await using var conn = new NpgsqlConnection(ConnStringWithFk);
        await conn.OpenAsync();
        
        for (int i = 0; i < 1000; i++)
        {
            var userId = _random.Next(1, 100001);
            var accountId = _random.Next(1, 250001);
            var merchantId = _random.Next(1, 5001);
            var categoryId = _random.Next(1, 11);
            var amount = (decimal)(_random.NextDouble() * 1000);
            
            await using var cmd = new NpgsqlCommand(
                "INSERT INTO transactions (user_id, account_id, merchant_id, category_id, amount, description, transaction_type, status) " +
                "VALUES (@userId, @accountId, @merchantId, @categoryId, @amount, @description, @type, @status)", conn);
            
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("accountId", accountId);
            cmd.Parameters.AddWithValue("merchantId", merchantId);
            cmd.Parameters.AddWithValue("categoryId", categoryId);
            cmd.Parameters.AddWithValue("amount", amount);
            cmd.Parameters.AddWithValue("description", "Transaction " + i);
            cmd.Parameters.AddWithValue("type", "PAYMENT");
            cmd.Parameters.AddWithValue("status", "COMPLETED");
            
            await cmd.ExecuteNonQueryAsync();
        }
    }

    [Benchmark(Description = "Sequential Insert WITHOUT FK")]
    public async Task SequentialInsert_WithoutFK()
    {
        await using var conn = new NpgsqlConnection(ConnStringWithoutFk);
        await conn.OpenAsync();
        
        for (int i = 0; i < 1000; i++)
        {
            var userId = _random.Next(1, 100001);
            var accountId = _random.Next(1, 250001);
            var merchantId = _random.Next(1, 5001);
            var categoryId = _random.Next(1, 11);
            var amount = (decimal)(_random.NextDouble() * 1000);
            
            await using var cmd = new NpgsqlCommand(
                "INSERT INTO transactions (user_id, account_id, merchant_id, category_id, amount, description, transaction_type, status) " +
                "VALUES (@userId, @accountId, @merchantId, @categoryId, @amount, @description, @type, @status)", conn);
            
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("accountId", accountId);
            cmd.Parameters.AddWithValue("merchantId", merchantId);
            cmd.Parameters.AddWithValue("categoryId", categoryId);
            cmd.Parameters.AddWithValue("amount", amount);
            cmd.Parameters.AddWithValue("description", "Transaction " + i);
            cmd.Parameters.AddWithValue("type", "PAYMENT");
            cmd.Parameters.AddWithValue("status", "COMPLETED");
            
            await cmd.ExecuteNonQueryAsync();
        }
    }

    [Benchmark(Description = "Batch Insert WITH FK")]
    public async Task BatchInsert_WithFK()
    {
        await using var conn = new NpgsqlConnection(ConnStringWithFk);
        await conn.OpenAsync();
        
        await using var transaction = await conn.BeginTransactionAsync();
        
        for (int i = 0; i < 1000; i++)
        {
            var userId = _random.Next(1, 100001);
            var accountId = _random.Next(1, 250001);
            var merchantId = _random.Next(1, 5001);
            var categoryId = _random.Next(1, 11);
            var amount = (decimal)(_random.NextDouble() * 1000);
            
            await using var cmd = new NpgsqlCommand(
                "INSERT INTO transactions (user_id, account_id, merchant_id, category_id, amount, description, transaction_type, status) " +
                "VALUES (@userId, @accountId, @merchantId, @categoryId, @amount, @description, @type, @status)", conn, transaction);
            
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("accountId", accountId);
            cmd.Parameters.AddWithValue("merchantId", merchantId);
            cmd.Parameters.AddWithValue("categoryId", categoryId);
            cmd.Parameters.AddWithValue("amount", amount);
            cmd.Parameters.AddWithValue("description", "Transaction " + i);
            cmd.Parameters.AddWithValue("type", "PAYMENT");
            cmd.Parameters.AddWithValue("status", "COMPLETED");
            
            await cmd.ExecuteNonQueryAsync();
        }
        
        await transaction.CommitAsync();
    }

    [Benchmark(Description = "Batch Insert WITHOUT FK")]
    public async Task BatchInsert_WithoutFK()
    {
        await using var conn = new NpgsqlConnection(ConnStringWithoutFk);
        await conn.OpenAsync();
        
        await using var transaction = await conn.BeginTransactionAsync();
        
        for (int i = 0; i < 1000; i++)
        {
            var userId = _random.Next(1, 100001);
            var accountId = _random.Next(1, 250001);
            var merchantId = _random.Next(1, 5001);
            var categoryId = _random.Next(1, 11);
            var amount = (decimal)(_random.NextDouble() * 1000);
            
            await using var cmd = new NpgsqlCommand(
                "INSERT INTO transactions (user_id, account_id, merchant_id, category_id, amount, description, transaction_type, status) " +
                "VALUES (@userId, @accountId, @merchantId, @categoryId, @amount, @description, @type, @status)", conn, transaction);
            
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("accountId", accountId);
            cmd.Parameters.AddWithValue("merchantId", merchantId);
            cmd.Parameters.AddWithValue("categoryId", categoryId);
            cmd.Parameters.AddWithValue("amount", amount);
            cmd.Parameters.AddWithValue("description", "Transaction " + i);
            cmd.Parameters.AddWithValue("type", "PAYMENT");
            cmd.Parameters.AddWithValue("status", "COMPLETED");
            
            await cmd.ExecuteNonQueryAsync();
        }
        
        await transaction.CommitAsync();
    }

    [Benchmark(Description = "Concurrent Insert WITH FK")]
    public async Task ConcurrentInsert_WithFK()
    {
        var tasks = new List<Task>();
        
        for (int t = 0; t < 10; t++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await using var conn = new NpgsqlConnection(ConnStringWithFk);
                await conn.OpenAsync();
                
                for (int i = 0; i < 100; i++)
                {
                    var userId = _random.Next(1, 100001);
                    var accountId = _random.Next(1, 250001);
                    var merchantId = _random.Next(1, 5001);
                    var categoryId = _random.Next(1, 11);
                    var amount = (decimal)(_random.NextDouble() * 1000);
                    
                    await using var cmd = new NpgsqlCommand(
                        "INSERT INTO transactions (user_id, account_id, merchant_id, category_id, amount, description, transaction_type, status) " +
                        "VALUES (@userId, @accountId, @merchantId, @categoryId, @amount, @description, @type, @status)", conn);
                    
                    cmd.Parameters.AddWithValue("userId", userId);
                    cmd.Parameters.AddWithValue("accountId", accountId);
                    cmd.Parameters.AddWithValue("merchantId", merchantId);
                    cmd.Parameters.AddWithValue("categoryId", categoryId);
                    cmd.Parameters.AddWithValue("amount", amount);
                    cmd.Parameters.AddWithValue("description", "Transaction " + i);
                    cmd.Parameters.AddWithValue("type", "PAYMENT");
                    cmd.Parameters.AddWithValue("status", "COMPLETED");
                    
                    await cmd.ExecuteNonQueryAsync();
                }
            }));
        }
        
        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "Concurrent Insert WITHOUT FK")]
    public async Task ConcurrentInsert_WithoutFK()
    {
        var tasks = new List<Task>();
        
        for (int t = 0; t < 10; t++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await using var conn = new NpgsqlConnection(ConnStringWithoutFk);
                await conn.OpenAsync();
                
                for (int i = 0; i < 100; i++)
                {
                    var userId = _random.Next(1, 100001);
                    var accountId = _random.Next(1, 250001);
                    var merchantId = _random.Next(1, 5001);
                    var categoryId = _random.Next(1, 11);
                    var amount = (decimal)(_random.NextDouble() * 1000);
                    
                    await using var cmd = new NpgsqlCommand(
                        "INSERT INTO transactions (user_id, account_id, merchant_id, category_id, amount, description, transaction_type, status) " +
                        "VALUES (@userId, @accountId, @merchantId, @categoryId, @amount, @description, @type, @status)", conn);
                    
                    cmd.Parameters.AddWithValue("userId", userId);
                    cmd.Parameters.AddWithValue("accountId", accountId);
                    cmd.Parameters.AddWithValue("merchantId", merchantId);
                    cmd.Parameters.AddWithValue("categoryId", categoryId);
                    cmd.Parameters.AddWithValue("amount", amount);
                    cmd.Parameters.AddWithValue("description", "Transaction " + i);
                    cmd.Parameters.AddWithValue("type", "PAYMENT");
                    cmd.Parameters.AddWithValue("status", "COMPLETED");
                    
                    await cmd.ExecuteNonQueryAsync();
                }
            }));
        }
        
        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "Mixed Operations WITH FK")]
    public async Task MixedOperations_WithFK()
    {
        await using var conn = new NpgsqlConnection(ConnStringWithFk);
        await conn.OpenAsync();
        
        for (int i = 0; i < 500; i++)
        {
            var userId = _random.Next(1, 100001);
            var accountId = _random.Next(1, 250001);
            var merchantId = _random.Next(1, 5001);
            var categoryId = _random.Next(1, 11);
            var amount = (decimal)(_random.NextDouble() * 1000);
            
            // Insert
            await using var insertCmd = new NpgsqlCommand(
                "INSERT INTO transactions (user_id, account_id, merchant_id, category_id, amount, description, transaction_type, status) " +
                "VALUES (@userId, @accountId, @merchantId, @categoryId, @amount, @description, @type, @status)", conn);
            
            insertCmd.Parameters.AddWithValue("userId", userId);
            insertCmd.Parameters.AddWithValue("accountId", accountId);
            insertCmd.Parameters.AddWithValue("merchantId", merchantId);
            insertCmd.Parameters.AddWithValue("categoryId", categoryId);
            insertCmd.Parameters.AddWithValue("amount", amount);
            insertCmd.Parameters.AddWithValue("description", "Transaction " + i);
            insertCmd.Parameters.AddWithValue("type", "PAYMENT");
            insertCmd.Parameters.AddWithValue("status", "COMPLETED");
            
            await insertCmd.ExecuteNonQueryAsync();
            
            // Read
            await using var selectCmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM transactions WHERE user_id = @userId", conn);
            selectCmd.Parameters.AddWithValue("userId", userId);
            
            await selectCmd.ExecuteScalarAsync();
        }
    }

    [Benchmark(Description = "Mixed Operations WITHOUT FK")]
    public async Task MixedOperations_WithoutFK()
    {
        await using var conn = new NpgsqlConnection(ConnStringWithoutFk);
        await conn.OpenAsync();
        
        for (int i = 0; i < 500; i++)
        {
            var userId = _random.Next(1, 100001);
            var accountId = _random.Next(1, 250001);
            var merchantId = _random.Next(1, 5001);
            var categoryId = _random.Next(1, 11);
            var amount = (decimal)(_random.NextDouble() * 1000);
            
            // Insert
            await using var insertCmd = new NpgsqlCommand(
                "INSERT INTO transactions (user_id, account_id, merchant_id, category_id, amount, description, transaction_type, status) " +
                "VALUES (@userId, @accountId, @merchantId, @categoryId, @amount, @description, @type, @status)", conn);
            
            insertCmd.Parameters.AddWithValue("userId", userId);
            insertCmd.Parameters.AddWithValue("accountId", accountId);
            insertCmd.Parameters.AddWithValue("merchantId", merchantId);
            insertCmd.Parameters.AddWithValue("categoryId", categoryId);
            insertCmd.Parameters.AddWithValue("amount", amount);
            insertCmd.Parameters.AddWithValue("description", "Transaction " + i);
            insertCmd.Parameters.AddWithValue("type", "PAYMENT");
            insertCmd.Parameters.AddWithValue("status", "COMPLETED");
            
            await insertCmd.ExecuteNonQueryAsync();
            
            // Read
            await using var selectCmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM transactions WHERE user_id = @userId", conn);
            selectCmd.Parameters.AddWithValue("userId", userId);
            
            await selectCmd.ExecuteScalarAsync();
        }
    }
}
