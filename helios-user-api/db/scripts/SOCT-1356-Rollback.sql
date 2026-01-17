-- Add  constraint to check primary mobile number condition in huser.phone_number
DO $$
BEGIN
    -- Check if the constraint 'chk_is_primary_phone_type' does not already exist
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.table_constraints 
        WHERE table_schema = 'huser'
          AND table_name = 'phone_number'
          AND constraint_name = 'chk_is_primary_phone_type'
    ) THEN
        -- Add the constraint to enforce the primary phone number conditions
        ALTER TABLE huser.phone_number
        ADD CONSTRAINT chk_is_primary_phone_type
        CHECK (
            (phone_type_id = 2) OR  -- Allows both TRUE and FALSE for is_primary when phone_type_id = 2
            (phone_type_id != 2 AND is_primary = FALSE)  -- Enforces is_primary = FALSE for other types
        );
    END IF;
END $$;