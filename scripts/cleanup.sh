#!/bin/bash

echo "🧹 Cleaning up benchmark databases..."

# Get the script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_ROOT"

# Stop and remove containers
docker compose down -v

echo "✅ Cleanup complete!"
