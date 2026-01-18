#!/bin/bash

echo "🌱 Seeding transactions for READ benchmarks..."

# Seed WITH FK database
echo "📝 Seeding database WITH FK..."
docker exec -i postgres-with-fk psql -U benchmark -d benchmark_with_fk << 'SQL'
-- Insert 1 million transactions
INSERT INTO transactions (user_id, account_id, merchant_id, category_id, amount, description, transaction_type, status)
SELECT 
    (random() * 99999 + 1)::bigint AS user_id,
    (SELECT id FROM accounts ORDER BY random() LIMIT 1) AS account_id,
    (SELECT id FROM merchants ORDER BY random() LIMIT 1) AS merchant_id,
    (SELECT id FROM categories ORDER BY random() LIMIT 1) AS category_id,
    (random() * 500 + 10)::decimal(18,2) AS amount,
    'Transaction ' || generate_series AS description,
    CASE (random() * 2)::int
        WHEN 0 THEN 'PURCHASE'
        WHEN 1 THEN 'REFUND'
        ELSE 'TRANSFER'
    END AS transaction_type,
    CASE (random() * 2)::int
        WHEN 0 THEN 'COMPLETED'
        WHEN 1 THEN 'PENDING'
        ELSE 'FAILED'
    END AS status
FROM generate_series(1, 1000000);

ANALYZE transactions;

SELECT COUNT(*) AS total_transactions FROM transactions;
SQL

# Seed WITHOUT FK database
echo "📝 Seeding database WITHOUT FK..."
docker exec -i postgres-without-fk psql -U benchmark -d benchmark_without_fk << 'SQL'
-- Insert 1 million transactions
INSERT INTO transactions (user_id, account_id, merchant_id, category_id, amount, description, transaction_type, status)
SELECT 
    (random() * 99999 + 1)::bigint AS user_id,
    (SELECT id FROM accounts ORDER BY random() LIMIT 1) AS account_id,
    (SELECT id FROM merchants ORDER BY random() LIMIT 1) AS merchant_id,
    (SELECT id FROM categories ORDER BY random() LIMIT 1) AS category_id,
    (random() * 500 + 10)::decimal(18,2) AS amount,
    'Transaction ' || generate_series AS description,
    CASE (random() * 2)::int
        WHEN 0 THEN 'PURCHASE'
        WHEN 1 THEN 'REFUND'
        ELSE 'TRANSFER'
    END AS transaction_type,
    CASE (random() * 2)::int
        WHEN 0 THEN 'COMPLETED'
        WHEN 1 THEN 'PENDING'
        ELSE 'FAILED'
    END AS status
FROM generate_series(1, 1000000);

ANALYZE transactions;

SELECT COUNT(*) AS total_transactions FROM transactions;
SQL

echo ""
echo "✅ Transactions seeded successfully!"
echo "📊 1,000,000 transactions inserted in each database"
echo ""
echo "🚀 Now you can run the READ benchmarks:"
echo "   cd ReadBenchmarks && dotnet run -c Release"
