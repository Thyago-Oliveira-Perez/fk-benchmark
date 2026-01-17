#!/bin/bash

echo "🚀 Starting Foreign Key Performance Benchmark Setup (Multiple FKs)"
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi

echo "✅ Docker is running"
echo ""

# Get the script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Start PostgreSQL containers
echo "📦 Starting PostgreSQL containers..."
cd "$PROJECT_ROOT"
docker compose up -d

echo "⏳ Waiting for databases to be ready..."
sleep 10

echo "🔧 Setting up database WITH Foreign Keys (4 FKs per transaction)..."
docker exec -i postgres-with-fk psql -U benchmark -d benchmark_with_fk < "$PROJECT_ROOT/sql/setup-with-fk.sql"

echo "🔧 Setting up database WITHOUT Foreign Keys..."
docker exec -i postgres-without-fk psql -U benchmark -d benchmark_without_fk < "$PROJECT_ROOT/sql/setup-without-fk.sql"

echo ""
echo "✅ Setup complete!"
echo ""
echo "📊 Schema includes:"
echo "   - 100,000 users"
echo "   - 250,000 accounts"
echo "   - 5,000 merchants"
echo "   - 10 categories"
echo "   - 4 Foreign Keys per transaction (WITH FK database)"
echo ""
echo "📊 To run the benchmark, execute:"
echo "   cd FkBenchmark && dotnet run -c Release"
echo ""
echo "🧹 To clean up, execute:"
echo "   cd scripts && ./cleanup.sh"
