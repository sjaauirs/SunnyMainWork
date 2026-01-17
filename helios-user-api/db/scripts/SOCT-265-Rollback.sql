-- Drop the constraint chk_is_primary_address_type from huser.person_address if it exists
DO $$
BEGIN
    -- Check if the constraint 'chk_is_primary_address_type' exists
    IF EXISTS (
        SELECT 1 
        FROM information_schema.table_constraints 
        WHERE table_schema = 'huser'
          AND table_name = 'person_address'
          AND constraint_name = 'chk_is_primary_address_type'
    ) THEN
        -- Drop the constraint if it exists
        ALTER TABLE huser.person_address
        DROP CONSTRAINT chk_is_primary_address_type;
    END IF;
END $$;