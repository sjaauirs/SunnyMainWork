DO $$
BEGIN
    -- Check if the column 'agreement_file_name' exists in 'huser.consumer'
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'agreement_file_name'
    ) THEN
        -- Drop the column if it exists
        ALTER TABLE huser.consumer
        DROP COLUMN agreement_file_name;
    END IF;
END $$;
