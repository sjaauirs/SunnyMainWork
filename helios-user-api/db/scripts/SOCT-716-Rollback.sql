DO $$
BEGIN
    -- Check if the column 'member_type' exists in 'huser.consumer'
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'member_type'
    ) THEN
        -- Drop the column if it exists
        ALTER TABLE huser.consumer
        DROP COLUMN member_type;
    END IF;
END $$;
