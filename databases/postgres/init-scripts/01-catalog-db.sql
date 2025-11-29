-- =====================================================
-- Catalog Database Initialization
-- =====================================================
-- This script runs automatically when the database is first created
-- It sets up indexes, constraints, and seed data specific to CatalogDb
-- =====================================================

\c CatalogDb;

-- =====================================================
-- Enable Extensions
-- =====================================================

-- UUID generation (if needed)
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Full-text search (if needed)
-- CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- =====================================================
-- Custom Functions (if needed)
-- =====================================================

-- Example: Function to update modified timestamp
CREATE OR REPLACE FUNCTION update_modified_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.modified_at = now();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- =====================================================
-- Seed Data (Optional - EF Core migrations handle this)
-- =====================================================

-- Note: EF Core migrations will create tables and seed data
-- This script is for database-specific setup only

-- =====================================================
-- Monitoring Views (Optional)
-- =====================================================

-- Example: View for monitoring table sizes
CREATE OR REPLACE VIEW table_sizes AS
SELECT
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size,
    pg_total_relation_size(schemaname||'.'||tablename) AS size_bytes
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- =====================================================
-- Permissions (if needed)
-- =====================================================

-- Grant necessary permissions
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO postgres;

-- =====================================================
-- Completion Message
-- =====================================================

DO $$
BEGIN
    RAISE NOTICE 'CatalogDb initialized successfully!';
END $$;
