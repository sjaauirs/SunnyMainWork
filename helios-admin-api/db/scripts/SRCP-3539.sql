DO $$
BEGIN
    -- Check if the table 'admin.script' exists
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'admin'
          AND table_name = 'script'
    ) THEN
        -- Alter the column to make it NOT NULL if the table exists
        ALTER TABLE admin.script
        ALTER COLUMN script_source SET NOT NULL;
    END IF;
END $$;
