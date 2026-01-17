DO $$
BEGIN
    -- Drop last_loaded_at column if it exists
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'etl'
          AND table_name = 'redshift_sync_status'
          AND column_name = 'last_loaded_at'
    ) THEN
        ALTER TABLE etl.redshift_sync_status
        DROP COLUMN last_loaded_at;
    END IF;

    -- Add last_loaded_id column if it doesn't exist
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'etl'
          AND table_name = 'redshift_sync_status'
          AND column_name = 'last_loaded_id'
    ) THEN
        ALTER TABLE etl.redshift_sync_status
        ADD COLUMN last_loaded_id bigint NOT NULL DEFAULT 0;
    END IF;
END $$;
