DO $$
BEGIN
    -- Check if the table 'admin.script' exists
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'admin'
          AND table_name = 'script'
    ) THEN
        -- Alter the column to make it NULLABLE again
        ALTER TABLE admin.script
        ALTER COLUMN script_source DROP NOT NULL;
    END IF;
END $$;
