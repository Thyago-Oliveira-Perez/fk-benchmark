# Foreign Key Performance Benchmark

> _What happens when you question database dogma?_

This project was born from a simple shock: discovering a production `transactions` table without Foreign Keys. This seemed to contradict everything taught in database courses. When I asked our CTO why, his answer was simple: **"Performance."**

That response motivated me to spend a weekend building comprehensive benchmarks to measure the **real, measurable impact** of Foreign Keys on common database operations.

**Technologies used:**
- **.NET 8** (production stack)
- **BenchmarkDotNet 0.15.8** (rigorous statistical measurement)
- **PostgreSQL 16** (industry-standard RDBMS)
- **Docker** (reproducible environments)

**What I discovered:**
- ✅ **Part 1 (INSERT)**: 0.16%-32.7% overhead (workload-dependent)
- ✅ **Part 2 (SELECT)**: ~0% overhead (< 1% difference)
- ✅ **Part 3 (DELETE)**: CASCADE 24% slower at 10K scale (NOT 20×), Soft Delete 33× faster

**The conclusion**: Context matters more than rules. Measure, don't assume.

## 📋 Prerequisites

- Docker and Docker Compose
- .NET 8 SDK
- mise (for .NET version management - optional)

## 🚀 Quick Start

```bash
# Clone the repository
git clone https://github.com/Thyago-Oliveira-Perez/fk-benchmark.git
cd fk-benchmark

# Setup databases and seed data
cd scripts
./setup.sh

# Run Part 1: INSERT benchmarks
cd ../InsertBenchmarks
dotnet run -c Release

# Run Part 2: SELECT benchmarks
cd ../ReadBenchmarks
dotnet run -c Release

# Run Part 3: DELETE benchmarks
cd ../DeleteBenchmarks
dotnet run -c Release

# Clean up when done
cd ../scripts
./cleanup.sh
```

## 📊 What It Tests

### Part 1: INSERT Operations

Located in `InsertBenchmarks/InsertBench.cs`

The benchmark tests **4 Foreign Key validations** per insert:

```sql
CREATE TABLE transactions (
    user_id BIGINT NOT NULL,      -- FK #1 → users
    account_id BIGINT NOT NULL,   -- FK #2 → accounts
    merchant_id BIGINT NOT NULL,  -- FK #3 → merchants
    category_id BIGINT NOT NULL,  -- FK #4 → categories
);
```

**Scenarios tested:**
- **Sequential Inserts**: Single-threaded (1,000 operations)
- **Batch Inserts**: Transactional bulk inserts (1,000 operations)
- **Concurrent Inserts**: Multi-threaded with 10 threads (1,000 total)
- **Mixed Operations**: Combined INSERT + SELECT (500 operations)

**Results:**
- Sequential: 5.5% overhead
- Batch: 32.7% overhead
- Concurrent: 0.16% overhead
- Mixed: 6.4% overhead

### Part 2: SELECT Operations

Located in `ReadBenchmarks/ReadOperationsBench.cs`

**Scenarios tested:**
- **Simple SELECT**: Query by FK column with LIMIT (0.75ms vs 0.88ms)
- **Multi-Table JOIN**: 4-way JOIN to enrich transaction data (1.62ms - identical)
- **Aggregation Query**: SUM/COUNT with GROUP BY (0.86ms vs 0.82ms)
- **Complex Analytical**: Date grouping + aggregation (23.26ms vs 23.51ms)

**Key Finding**: Foreign Keys have ~0% impact on SELECT queries (< 1% difference)

### Part 3: DELETE Operations

Located in `DeleteBenchmarks/DeleteBench.cs`

**Scenarios tested:**
- **Small volume**: 100 transactions per user
- **Medium volume**: 1,000 transactions per user
- **Large volume**: 10,000 transactions per user
- **Soft Delete**: UPDATE deleted_at (instant)

**Comparisons:**
- CASCADE delete (WITH FK)
- Manual delete (WITHOUT FK)
- Soft delete (both databases)

**Results:**
- 100 txns: CASCADE 6% faster (44.69ms vs 47.66ms)
- 1K txns: Manual 30% faster (43.18ms vs 61.91ms)
- 10K txns: Manual 24% faster (249.79ms vs 326.73ms)
- Soft Delete: 33× faster than CASCADE (9.75ms vs 326.73ms)

## 🗂️ Project Structure

```
fk-benchmark/
├── docker-compose.yml              # PostgreSQL containers (WITH/WITHOUT FK)
├── fk-benchmark.sln                # Solution with 3 projects
├── .mise.toml                      # .NET 8 configuration
├── README.md                       # This file
├── sql/
│   ├── setup-with-fk.sql          # Schema WITH 4 Foreign Keys
│   └── setup-without-fk.sql       # Schema WITHOUT Foreign Keys
├── scripts/
│   ├── setup.sh                   # Automated setup
│   ├── cleanup.sh                 # Cleanup all resources
│   └── seed-transactions.sh       # Seed 1M transactions
├── InsertBenchmarks/              # Part 1: INSERT operations
│   ├── Program.cs
│   ├── InsertBench.cs
│   └── InsertBenchmarks.csproj
├── ReadBenchmarks/                # Part 2: SELECT operations
│   ├── Program.cs
│   ├── ReadOperationsBench.cs
│   └── ReadBenchmarks.csproj
└── DeleteBenchmarks/              # Part 3: DELETE operations
    ├── Program.cs
    ├── DeleteBench.cs
    └── DeleteBenchmarks.csproj
```

## 📊 Understanding Results

BenchmarkDotNet outputs detailed statistics:

- **Mean**: Average execution time
- **Error**: Standard error of the mean
- **StdDev**: Standard deviation
- **Allocated**: Memory allocation per operation

**Expected results:**
- **Part 1 (INSERT)**: 0.16%-32.7% overhead WITH FK (workload-dependent)
- **Part 2 (SELECT)**: ~0% overhead WITH FK (indexes matter, not constraints)
- **Part 3 (DELETE)**: 24% slower CASCADE at 10K scale (NOT 20×), Soft Delete 33× faster

## 🛠️ Scripts

### setup.sh
Automates setup:
- Starts PostgreSQL containers (WITH FK on 5432, WITHOUT FK on 5433)
- Waits for databases to be ready
- Seeds data:
  - 100,000 users
  - 250,000 accounts
  - 5,000 merchants
  - 10 categories

### seed-transactions.sh
Seeds 1M transactions for READ benchmarks:
- 1,000,000 transactions distributed across users
- Transaction types: PURCHASE, REFUND, TRANSFER
- Status: COMPLETED, PENDING, FAILED

### cleanup.sh
Removes all resources:
- Stops containers
- Removes volumes
- Cleans test data

## 🔧 Configuration

### Database Configuration
Both PostgreSQL 16 instances run with identical settings:
- Shared buffers: 256MB
- Max connections: 200
- Work memory: 16MB
- Effective cache size: 1GB

### Connection Strings
- **With FK**: `localhost:5432` 
- **Without FK**: `localhost:5433`
- **User**: `benchmark` / **Password**: `benchmark123`

## 📝 Test Data

- **100,000 users** (id, email, name, created_at, deleted_at)
- **250,000 accounts** (2.5 accounts per user on average)
- **5,000 merchants** (retail, restaurant, online, services)
- **10 categories** (groceries, dining, entertainment, etc.)
- **1,000,000 transactions** (for READ benchmarks)
- **Indexes** on all FK columns (both databases)
- **Partial index** on deleted_at WHERE deleted_at IS NULL

## 🎯 Benchmarking Tips

1. **Run in Release mode** (`dotnet run -c Release`)
2. **Close other applications** to reduce noise
3. **Run multiple times** for consistency
4. **Note your hardware** when sharing results
5. **Check Docker resources** (ensure adequate CPU/memory)
6. **Part 3 uses real COMMITs** (no ROLLBACK, measures true cost)

## 🐛 Troubleshooting

### Databases won't start
```bash
# Check if ports are in use
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

# Verify ready
docker exec postgres-with-fk pg_isready -U benchmark
```

### Benchmark fails
```bash
# Rebuild
dotnet clean
dotnet restore
dotnet build -c Release
```

### Missing transactions for Part 2
```bash
# Seed 1M transactions
cd scripts
./seed-transactions.sh
```

## 📚 Related Articles

This benchmark accompanies a three-part article series that documents my journey from shock to understanding:

**[Part 1: The INSERT Story](/posts/foreign-keys-vs-performance-part-1)**
- The shock of finding a production DB without FKs
- FK overhead on write operations (0.16%-32.7%)
- Sequential vs Batch vs Concurrent
- When to remove Foreign Keys

**[Part 2: The SELECT Story](/posts/foreign-keys-vs-performance-part-2)**
- Do FKs slow down SELECT queries? (~0% impact!)
- Indexes matter, constraints don't
- Read-heavy systems should keep FKs

**[Part 3: The DELETE Story](/posts/foreign-keys-vs-performance-part-3)**
- Busting the CASCADE "20× slower" myth (only 24%)
- Soft delete is 33× faster (9.75ms vs 326.73ms)
- Lock contention considerations
- My final conclusion: question everything

**The journey**: From academic assumptions to production reality, measured with real benchmarks.

## 🤝 Contributing

Contributions welcome:
- New benchmark scenarios
- Different database versions
- Other databases (MySQL, SQL Server, etc.)
- Share your results with hardware specs

## 📄 License

MIT License - Free to use and modify.
