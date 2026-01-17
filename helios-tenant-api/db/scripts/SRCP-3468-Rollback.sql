-- Script to remove the utc_time_offset column from the tenant.tenant table
-- This will drop the column if it exists
-- Ensure that removing this column is safe for your data and application

DO $$
BEGIN
    -- Check if the utc_time_offset column exists in the tenant.tenant table
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'tenant' 
          AND table_name = 'tenant' 
          AND column_name = 'utc_time_offset'
    ) THEN
        -- Drop the utc_time_offset column from the table
        ALTER TABLE tenant.tenant
        DROP COLUMN utc_time_offset;
        
        -- Log success message after dropping the column
        RAISE NOTICE 'The column "utc_time_offset" has been successfully dropped from the table "tenant.tenant".';
    ELSE
        -- Log a message that the column does not exist
        RAISE NOTICE 'The column "utc_time_offset" does not exist in the table "tenant.tenant".';
    END IF;
END $$;

-- End of script for utc_time_offset column removal


-- Script to remove the dst_enabled column from the tenant.tenant table
-- This will drop the column if it exists
-- Ensure that removing this column is safe for your data and application

DO $$
BEGIN
    -- Check if the dst_enabled column exists in the tenant.tenant table
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'tenant' 
          AND table_name = 'tenant' 
          AND column_name = 'dst_enabled'
    ) THEN
        -- Drop the dst_enabled column from the table
        ALTER TABLE tenant.tenant
        DROP COLUMN dst_enabled;
        
        -- Log success message after dropping the column
        RAISE NOTICE 'The column "dst_enabled" has been successfully dropped from the table "tenant.tenant".';
    ELSE
        -- Log a message that the column does not exist
        RAISE NOTICE 'The column "dst_enabled" does not exist in the table "tenant.tenant".';
    END IF;
END $$;

-- End of script for dst_enabled column removal
