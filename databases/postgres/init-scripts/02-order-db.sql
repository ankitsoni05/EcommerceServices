-- =====================================================
-- Order Database Initialization (Future)
-- =====================================================

\c OrderDb;

-- Enable extensions
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Custom functions and triggers
CREATE OR REPLACE FUNCTION update_modified_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.modified_at = now();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Permissions
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;

DO $$
BEGIN
    RAISE NOTICE 'OrderDb initialized successfully!';
END $$;
