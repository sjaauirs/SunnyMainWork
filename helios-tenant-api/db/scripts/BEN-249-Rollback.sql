-- Rollback script
DO $$
BEGIN
    -- Drop wallet_category table if exists
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'tenant'
          AND table_name   = 'wallet_category'
    ) THEN
        EXECUTE 'DROP TABLE tenant.wallet_category CASCADE';
    END IF;

    -- Drop category table if exists
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'tenant'
          AND table_name   = 'category'
    ) THEN
        EXECUTE 'DROP TABLE tenant.category CASCADE';
    END IF;
END $$;
