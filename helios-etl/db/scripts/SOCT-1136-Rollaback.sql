-- Rollback: remove last_loaded_id column
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'etl'
          AND table_name = 'redshift_sync_status'
          AND column_name = 'last_loaded_id'
    ) THEN
        ALTER TABLE etl.redshift_sync_status
        DROP COLUMN last_loaded_id;
    END IF;
END $$;
