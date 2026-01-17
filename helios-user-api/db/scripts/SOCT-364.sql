DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'agreement_file_name'
    ) THEN
        ALTER TABLE huser.consumer
        ADD COLUMN agreement_file_name VARCHAR(255) NULL;
    END IF;
END
$$;
