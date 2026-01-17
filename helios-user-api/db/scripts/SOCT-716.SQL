DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'member_type'
    ) THEN
        -- Add column as nullable
        ALTER TABLE huser.consumer
        ADD COLUMN member_type varchar(225);

        -- Set default value for existing rows
        UPDATE huser.consumer
        SET member_type = 'SU'
        WHERE TRIM(COALESCE(member_type, '')) = '';

        -- Set column to NOT NULL after populating
        ALTER TABLE huser.consumer
        ALTER COLUMN member_type SET NOT NULL;
    END IF;
END
$$;
