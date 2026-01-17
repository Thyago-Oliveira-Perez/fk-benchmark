# Foreign Key Performance Benchmark

This project benchmarks the performance impact of Foreign Key constraints in high-transaction scenarios using .NET 10, BenchmarkDotNet, and PostgreSQL 16.

**Schema Version**: **4 Foreign Keys** per transaction for realistic testing.

## 📋 Prerequisites

- Docker and Docker Compose
- .NET 10 SDK (or .NET 8+)
- mise (for .NET version management - optional)

## 🚀 Quick Start

We provide automated scripts for easy setup and cleanup:

```bash
# Run the setup script to start databases and initialize data
cd scripts
./setup.sh

# Run the benchmark
cd ../FkBenchmark
dotnet run -c Release

# Clean up when done
cd ../scripts
./cleanup.sh
```

## 📊 What It Tests

The benchmark runs comprehensive tests with **4 Foreign Key validations** per insert:

- **Sequential Inserts**: Single-threaded inserts (1,000 operations)
- **Batch Inserts**: Transactional bulk inserts (1,000 operations)
- **Concurrent Inserts**: Multi-threaded inserts with 10 threads (1,000 total operations)
- **Mixed Operations**: Combined insert and select queries (500 operations)

### Schema: Realistic Multi-FK Design

Each transaction validates 4 Foreign Keys:

```sql
CREATE TABLE transactions (
    user_id BIGINT NOT NULL,      -- FK #1 → users
    account_id BIGINT NOT NULL,   -- FK #2 → accounts
    merchant_id BIGINT NOT NULL,  -- FK #3 → merchants
    category_id BIGINT NOT NULL,  -- FK #4 → categories
    -- 4 FK validations per INSERT!
);
```

Each scenario is tested against:
- ✅ Database **WITH** 4 Foreign Key constraints (port 5432)
- ❌ Database **WITHOUT** Foreign Key constraints (port 5433)

## �� Understanding Results

BenchmarkDotNet will output detailed statistics including:

- **Mean**: Average execution time
- **Error**: Standard error of the mean
- **StdDev**: Standard deviation
- **Rank**: Relative performance ranking
- **Gen0/Gen1/Gen2**: Garbage collection statistics
- **Allocated**: Memory allocation per operation

Example output:
```
|                          Method |     Mean |   Error |  StdDev | Rank |   Gen0 | Allocated |
|-------------------------------- |---------:|--------:|--------:|-----:|-------:|----------:|
| Batch Insert WITHOUT FK         | 145.2 ms | 2.8 ms  | 7.9 ms  |    1 | 1.2000 |   3.2 KB  |
| Batch Insert WITH FK            | 178.6 ms | 3.5 ms  | 10.1 ms |    2 | 1.5000 |   3.8 KB  |
```

**Expected Performance Gains**: With 4 FKs, you should see **15-30% faster** performance without FK constraints, especially in concurrent scenarios.

## 🗂️ Project Structure

```
fk-benchmark/
├── docker-compose.yml          # PostgreSQL containers configuration
├── .mise.toml                  # .NET version configuration
├── README.md                   # This file
├── sql/
│   ├── setup-with-fk.sql      # Schema WITH 4 Foreign Keys
│   └── setup-without-fk.sql   # Schema WITHOUT Foreign Keys
├── scripts/
│   ├── setup.sh               # Automated setup script
│   └── cleanup.sh             # Automated cleanup script
└── FkBenchmark/
    ├── Program.cs             # Entry point
    ├── TransactionBenchmarks.cs # Benchmark implementations
    └── FkBenchmark.csproj     # Project configuration
```

## 🛠️ Scripts

### setup.sh
Automates the entire setup process:
- Checks if Docker is running
- Starts PostgreSQL containers
- Waits for databases to be ready
- Initializes both databases with:
  - 100,000 users
  - 250,000 accounts
  - 5,000 merchants
  - 10 categories

### cleanup.sh
Removes all resources:
- Stops and removes Docker containers
- Deletes database volumes
- Cleans up all test data

## 🔧 Configuration

### Database Configuration
Both PostgreSQL instances run with identical configurations for fair comparison:
- Shared buffers: 256MB
- Max connections: 200
- Work memory: 16MB
- Maintenance work memory: 128MB
- Effective cache size: 1GB

### Connection Strings
- **With FK**: `localhost:5432` - Database: `benchmark_with_fk`
- **Without FK**: `localhost:5433` - Database: `benchmark_without_fk`
- **User**: `benchmark` / **Password**: `benchmark123`

## 📝 Test Data

Each database is seeded with:
- **100,000 users** (id, email, name, created_at)
- **250,000 accounts** (user_id, account_number, balance, account_type)
- **5,000 merchants** (name, category)
- **10 categories** (name, parent_category_id)
- Indexes on commonly queried fields
- Identical schema except for FK constraints

## 🧹 Clean Up

### Quick Cleanup
```bash
cd scripts
./cleanup.sh
```

### Manual Cleanup
```bash
# Stop and remove containers with volumes
docker compose down -v

# Verify containers are removed
docker ps -a | grep postgres
```

## 🎯 Benchmarking Tips

1. **Run in Release mode** for accurate performance metrics
2. **Close other applications** to reduce system noise
3. **Run multiple times** to ensure consistency
4. **Take screenshots** of results for comparison
5. **Note your hardware specs** when sharing results

## 🐛 Troubleshooting

### Databases won't start
```bash
# Check if ports are already in use
lsof -i :5432
lsof -i :5433

# Restart Docker
docker compose restart
```

### Connection errors
```bash
# Check database logs
docker logs postgres-with-fk
docker logs postgres-without-fk

# Verify databases are ready
docker exec postgres-with-fk pg_isready -U benchmark
```

### Benchmark fails
```bash
# Rebuild the project
cd FkBenchmark
dotnet clean
dotnet restore
dotnet build -c Release
```

## 📚 Related Article

This benchmark accompanies the article "Foreign Keys vs Performance: When Database Theory Meets Reality" which explores:
- Technical deep-dive into FK overhead
- Real-world performance impacts
- When to remove Foreign Keys
- Application-level integrity patterns

## 🤝 Contributing

Feel free to:
- Add new benchmark scenarios
- Test with different database configurations
- Compare other databases (MySQL, SQL Server, etc.)
- Share your results and hardware specs

## 📄 License

MIT License - Feel free to use and modify for your own benchmarking needs.
