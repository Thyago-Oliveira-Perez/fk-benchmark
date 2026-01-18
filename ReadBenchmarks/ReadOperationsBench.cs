using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Dapper;
using Npgsql;

namespace ReadBenchmarks;

[MemoryDiagnoser]
[SimpleJob(RunStrategy.Monitoring, iterationCount: 100)]
public class ReadOperationsBench
{
    private const string ConnectionStringWithFk = "Host=localhost;Port=5432;Database=benchmark_with_fk;Username=benchmark;Password=benchmark123";
    private const string ConnectionStringWithoutFk = "Host=localhost;Port=5433;Database=benchmark_without_fk;Username=benchmark;Password=benchmark123";
    private const int TestUserId = 50000; // User in the middle of dataset

    [Benchmark(Description = "Simple SELECT - WITH FK")]
    public async Task<List<Transaction>> SimpleSelect_WithFK()
    {
        await using var conn = new NpgsqlConnection(ConnectionStringWithFk);
        await conn.OpenAsync();

        var transactions = await conn.QueryAsync<Transaction>(@"
            SELECT * FROM transactions
            WHERE user_id = @UserId
            LIMIT 100
        ", new { UserId = TestUserId });

        return transactions.ToList();
    }

    [Benchmark(Description = "Simple SELECT - WITHOUT FK")]
    public async Task<List<Transaction>> SimpleSelect_WithoutFK()
    {
        await using var conn = new NpgsqlConnection(ConnectionStringWithoutFk);
        await conn.OpenAsync();

        var transactions = await conn.QueryAsync<Transaction>(@"
            SELECT * FROM transactions
            WHERE user_id = @UserId
            LIMIT 100
        ", new { UserId = TestUserId });

        return transactions.ToList();
    }

    [Benchmark(Description = "Multi-Table JOIN - WITH FK")]
    public async Task<List<EnrichedTransaction>> MultiTableJoin_WithFK()
    {
        await using var conn = new NpgsqlConnection(ConnectionStringWithFk);
        await conn.OpenAsync();

        var transactions = await conn.QueryAsync<EnrichedTransaction>(@"
            SELECT
                t.id,
                t.amount,
                t.description,
                u.name AS user_name,
                a.account_number,
                m.name AS merchant_name,
                c.name AS category_name
            FROM transactions t
            JOIN users u ON t.user_id = u.id
            JOIN accounts a ON t.account_id = a.id
            JOIN merchants m ON t.merchant_id = m.id
            JOIN categories c ON t.category_id = c.id
            WHERE t.user_id = @UserId
            LIMIT 100
        ", new { UserId = TestUserId });

        return transactions.ToList();
    }

    [Benchmark(Description = "Multi-Table JOIN - WITHOUT FK")]
    public async Task<List<EnrichedTransaction>> MultiTableJoin_WithoutFK()
    {
        await using var conn = new NpgsqlConnection(ConnectionStringWithoutFk);
        await conn.OpenAsync();

        var transactions = await conn.QueryAsync<EnrichedTransaction>(@"
            SELECT
                t.id,
                t.amount,
                t.description,
                u.name AS user_name,
                a.account_number,
                m.name AS merchant_name,
                c.name AS category_name
            FROM transactions t
            JOIN users u ON t.user_id = u.id
            JOIN accounts a ON t.account_id = a.id
            JOIN merchants m ON t.merchant_id = m.id
            JOIN categories c ON t.category_id = c.id
            WHERE t.user_id = @UserId
            LIMIT 100
        ", new { UserId = TestUserId });

        return transactions.ToList();
    }

    [Benchmark(Description = "Aggregation Query - WITH FK")]
    public async Task<List<CategorySummary>> AggregationQuery_WithFK()
    {
        await using var conn = new NpgsqlConnection(ConnectionStringWithFk);
        await conn.OpenAsync();

        var summary = await conn.QueryAsync<CategorySummary>(@"
            SELECT
                c.name,
                COUNT(*) AS transaction_count,
                SUM(t.amount) AS total_amount
            FROM transactions t
            JOIN categories c ON t.category_id = c.id
            WHERE t.user_id = @UserId
            GROUP BY c.id, c.name
            ORDER BY total_amount DESC
        ", new { UserId = TestUserId });

        return summary.ToList();
    }

    [Benchmark(Description = "Aggregation Query - WITHOUT FK")]
    public async Task<List<CategorySummary>> AggregationQuery_WithoutFK()
    {
        await using var conn = new NpgsqlConnection(ConnectionStringWithoutFk);
        await conn.OpenAsync();

        var summary = await conn.QueryAsync<CategorySummary>(@"
            SELECT
                c.name,
                COUNT(*) AS transaction_count,
                SUM(t.amount) AS total_amount
            FROM transactions t
            JOIN categories c ON t.category_id = c.id
            WHERE t.user_id = @UserId
            GROUP BY c.id, c.name
            ORDER BY total_amount DESC
        ", new { UserId = TestUserId });

        return summary.ToList();
    }

    [Benchmark(Description = "Complex Analytical - WITH FK")]
    public async Task<List<MonthlyAnalytics>> ComplexAnalytical_WithFK()
    {
        await using var conn = new NpgsqlConnection(ConnectionStringWithFk);
        await conn.OpenAsync();

        var analytics = await conn.QueryAsync<MonthlyAnalytics>(@"
            SELECT
                DATE_TRUNC('month', t.created_at) AS month,
                c.name AS category,
                COUNT(DISTINCT t.merchant_id) AS unique_merchants,
                COUNT(*) AS transaction_count,
                AVG(t.amount) AS avg_amount,
                SUM(t.amount) AS total_amount
            FROM transactions t
            JOIN categories c ON t.category_id = c.id
            WHERE t.user_id BETWEEN @StartUser AND @EndUser
            AND t.created_at >= NOW() - INTERVAL '12 months'
            GROUP BY DATE_TRUNC('month', t.created_at), c.id, c.name
            ORDER BY month DESC, total_amount DESC
        ", new { StartUser = 1000, EndUser = 2000 });

        return analytics.ToList();
    }

    [Benchmark(Description = "Complex Analytical - WITHOUT FK")]
    public async Task<List<MonthlyAnalytics>> ComplexAnalytical_WithoutFK()
    {
        await using var conn = new NpgsqlConnection(ConnectionStringWithoutFk);
        await conn.OpenAsync();

        var analytics = await conn.QueryAsync<MonthlyAnalytics>(@"
            SELECT
                DATE_TRUNC('month', t.created_at) AS month,
                c.name AS category,
                COUNT(DISTINCT t.merchant_id) AS unique_merchants,
                COUNT(*) AS transaction_count,
                AVG(t.amount) AS avg_amount,
                SUM(t.amount) AS total_amount
            FROM transactions t
            JOIN categories c ON t.category_id = c.id
            WHERE t.user_id BETWEEN @StartUser AND @EndUser
            AND t.created_at >= NOW() - INTERVAL '12 months'
            GROUP BY DATE_TRUNC('month', t.created_at), c.id, c.name
            ORDER BY month DESC, total_amount DESC
        ", new { StartUser = 1000, EndUser = 2000 });

        return analytics.ToList();
    }

    // DTOs
    public class Transaction
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long AccountId { get; set; }
        public long MerchantId { get; set; }
        public long CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class EnrichedTransaction
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }

    public class CategorySummary
    {
        public string Name { get; set; } = string.Empty;
        public long TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class MonthlyAnalytics
    {
        public DateTime Month { get; set; }
        public string Category { get; set; } = string.Empty;
        public long UniqueMerchants { get; set; }
        public long TransactionCount { get; set; }
        public decimal AvgAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
