DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'consumer' 
          AND table_schema = 'huser'
          AND column_name = 'anonymous_code'
    ) THEN
        ALTER TABLE huser.consumer
        ADD COLUMN anonymous_code varchar(50) NOT NULL DEFAULT ('anc-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', ''));
    END IF;
END $$;