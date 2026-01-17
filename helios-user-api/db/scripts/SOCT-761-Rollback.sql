-- Drop huser.phone_number table if it exists
DO $$
BEGIN
    IF EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'huser' AND table_name = 'phone_number'
    ) THEN
        DROP TABLE huser.phone_number;
    END IF;
END
$$;

-- Delete inserted phone types 'home' and 'mobile'
DELETE FROM huser.phone_type
WHERE LOWER(phone_type_name) IN ('home', 'mobile');

-- Drop indexes related to huser.phone_number (only if they exist)
DROP INDEX IF EXISTS one_primary_phone_number_per_person;
DROP INDEX IF EXISTS idx_phone_number_person_id;
DROP INDEX IF EXISTS idx_phone_number_person_primary;

-- Drop huser.phone_type table if it exists
DO $$
BEGIN
    IF EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'huser' AND table_name = 'phone_type'
    ) THEN
        DROP TABLE huser.phone_type;
    END IF;
END
$$;

-- Drop the constraint chk_is_primary_phone_type from huser.phone_number if it exists
DO $$
BEGIN
    -- Check if the constraint 'chk_is_primary_phone_type' exists
    IF EXISTS (
        SELECT 1 
        FROM information_schema.table_constraints 
        WHERE table_schema = 'huser'
          AND table_name = 'phone_number'
          AND constraint_name = 'chk_is_primary_phone_type'
    ) THEN
        -- Drop the constraint if it exists
        ALTER TABLE huser.phone_number
        DROP CONSTRAINT chk_is_primary_phone_type;
    END IF;
END $$;

-- Soft delete migrated phone numbers
DO $$ 
BEGIN
    -- Check if the phone_number table exists in the huser schema
    IF EXISTS (
        SELECT 1 
        FROM information_schema.tables 
        WHERE table_schema = 'huser' 
          AND table_name = 'phone_number'
    ) THEN
        -- Perform the soft delete
        UPDATE huser.phone_number 
        SET delete_nbr = phone_number_id,  -- Soft delete
            is_primary = FALSE              -- Unset primary
        WHERE source = 'ETL' 
          AND delete_nbr = 0;
    END IF;
END $$;