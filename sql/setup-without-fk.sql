-- Setup database WITHOUT Foreign Keys (More Realistic Scenario)

DROP TABLE IF EXISTS transaction_items CASCADE;
DROP TABLE IF EXISTS transactions CASCADE;
DROP TABLE IF EXISTS accounts CASCADE;
DROP TABLE IF EXISTS merchants CASCADE;
DROP TABLE IF EXISTS categories CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- Users table
CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    name VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_users_email ON users(email);

-- Accounts table (users can have multiple accounts)
CREATE TABLE accounts (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,  -- NO FK CONSTRAINT
    account_number VARCHAR(50) NOT NULL UNIQUE,
    balance DECIMAL(18,2) DEFAULT 0,
    account_type VARCHAR(50) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_accounts_user_id ON accounts(user_id);

-- Merchants table
CREATE TABLE merchants (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    category VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Categories table
CREATE TABLE categories (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    parent_category_id BIGINT,  -- NO FK CONSTRAINT
    created_at TIMESTAMP DEFAULT NOW()
);

-- Transactions table WITHOUT Foreign Keys
CREATE TABLE transactions (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,      -- NO FK CONSTRAINT
    account_id BIGINT NOT NULL,   -- NO FK CONSTRAINT
    merchant_id BIGINT NOT NULL,  -- NO FK CONSTRAINT
    category_id BIGINT NOT NULL,  -- NO FK CONSTRAINT
    amount DECIMAL(18,2) NOT NULL,
    description VARCHAR(500),
    transaction_type VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Transaction Items (line items for each transaction)
CREATE TABLE transaction_items (
    id BIGSERIAL PRIMARY KEY,
    transaction_id BIGINT NOT NULL,  -- NO FK CONSTRAINT
    product_name VARCHAR(255) NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(18,2) NOT NULL
);

-- Indexes (same as with FK for fair comparison)
CREATE INDEX idx_transactions_user_id ON transactions(user_id);
CREATE INDEX idx_transactions_account_id ON transactions(account_id);
CREATE INDEX idx_transactions_merchant_id ON transactions(merchant_id);
CREATE INDEX idx_transactions_category_id ON transactions(category_id);
CREATE INDEX idx_transactions_created_at ON transactions(created_at);
CREATE INDEX idx_transactions_status ON transactions(status);
CREATE INDEX idx_transaction_items_transaction_id ON transaction_items(transaction_id);

-- Insert seed data (IDENTICAL to with-fk version)
-- 100,000 users
INSERT INTO users (email, name)
SELECT 
    'user' || generate_series || '@example.com',
    'User ' || generate_series
FROM generate_series(1, 100000);

-- 10 categories
INSERT INTO categories (name) VALUES 
    ('Food & Dining'), ('Shopping'), ('Transportation'), ('Entertainment'),
    ('Bills & Utilities'), ('Health & Fitness'), ('Travel'), ('Education'),
    ('Personal Care'), ('Groceries');

-- 5,000 merchants
INSERT INTO merchants (name, category)
SELECT 
    'Merchant ' || generate_series,
    CASE (generate_series % 10)
        WHEN 0 THEN 'Food & Dining'
        WHEN 1 THEN 'Shopping'
        WHEN 2 THEN 'Transportation'
        WHEN 3 THEN 'Entertainment'
        WHEN 4 THEN 'Bills & Utilities'
        WHEN 5 THEN 'Health & Fitness'
        WHEN 6 THEN 'Travel'
        WHEN 7 THEN 'Education'
        WHEN 8 THEN 'Personal Care'
        ELSE 'Groceries'
    END
FROM generate_series(1, 5000);

-- 250,000 accounts (2-3 accounts per user on average)
INSERT INTO accounts (user_id, account_number, account_type, balance)
SELECT 
    (random() * 99999 + 1)::bigint,
    'ACC' || generate_series || '-' || LPAD((random() * 9999)::text, 4, '0'),
    CASE (generate_series % 3)
        WHEN 0 THEN 'CHECKING'
        WHEN 1 THEN 'SAVINGS'
        ELSE 'CREDIT'
    END,
    (random() * 10000)::decimal(18,2)
FROM generate_series(1, 250000);

-- Analyze tables for query optimizer
ANALYZE users;
ANALYZE accounts;
ANALYZE merchants;
ANALYZE categories;
ANALYZE transactions;
ANALYZE transaction_items;

-- Show table info
SELECT 
    'Setup complete!' as message,
    (SELECT COUNT(*) FROM users) as total_users,
    (SELECT COUNT(*) FROM accounts) as total_accounts,
    (SELECT COUNT(*) FROM merchants) as total_merchants,
    (SELECT COUNT(*) FROM categories) as total_categories,
    (SELECT COUNT(*) FROM transactions) as total_transactions;
