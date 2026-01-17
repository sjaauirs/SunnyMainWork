-- Script to add the utc_time_offset column to the tenant.tenant table
-- Ensures the column is added only if it doesn't already exist
-- Contains timezone strings, e.g., UTC-6 (CST)

DO $$
BEGIN
    -- Check if the utc_time_offset column exists in the tenant.tenant table
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'tenant' 
          AND table_name = 'tenant' 
          AND column_name = 'utc_time_offset'
    ) THEN
        -- Add the utc_time_offset column to the table
        ALTER TABLE tenant.tenant
        ADD COLUMN utc_time_offset VARCHAR(80) NULL;
        
        -- Log success message
        RAISE NOTICE 'The column "utc_time_offset" has been successfully added to the table "tenant.tenant".';
    ELSE
        -- Log a message that the column already exists
        RAISE NOTICE 'The column "utc_time_offset" already exists in the table "tenant.tenant".';
    END IF;
END $$;

-- End of script

-- Script to add the dst_enabled column to the tenant.tenant table
-- Ensures the column is added only if it doesn't already exist
-- Indicates if DST is supported (TRUE for supported, FALSE for not supported)

DO $$
BEGIN
    -- Check if the dst_enabled column exists in the tenant.tenant table
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'tenant' 
          AND table_name = 'tenant' 
          AND column_name = 'dst_enabled'
    ) THEN
        -- Add the dst_enabled column to the table
        ALTER TABLE tenant.tenant
        ADD COLUMN dst_enabled BOOLEAN NULL;
        
        -- Log success message
        RAISE NOTICE 'The column "dst_enabled" has been successfully added to the table "tenant.tenant".';
    ELSE
        -- Log a message that the column already exists
        RAISE NOTICE 'The column "dst_enabled" already exists in the table "tenant.tenant".';
    END IF;
END $$;

-- End of script
