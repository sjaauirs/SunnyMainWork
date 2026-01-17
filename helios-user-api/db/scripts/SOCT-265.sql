-- Add modified constraint to check primary address condition in huser.person_address
DO $$
BEGIN
    -- Check if the constraint 'chk_is_primary_address_type' does not already exist
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.table_constraints 
        WHERE table_schema = 'huser'
          AND table_name = 'person_address'
          AND constraint_name = 'chk_is_primary_address_type'
    ) THEN
        -- Add the constraint to enforce the primary address conditions
        ALTER TABLE huser.person_address
        ADD CONSTRAINT chk_is_primary_address_type
        CHECK (
			(address_type_id = 1) OR  -- Allows both TRUE and FALSE for is_primary when address_type_id = 1
            (address_type_id != 1 AND is_primary = FALSE)  -- Enforces is_primary = FALSE for address_type_id != 1
        );
    END IF;
END $$;