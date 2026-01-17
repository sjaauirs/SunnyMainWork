-- Soft delete migrated address rows
DO $$ 
DECLARE 
    mailing_address_type_id BIGINT; 
BEGIN 
    -- Step 1: Get the address_type_id for 'mailing' 
    SELECT address_type_id 
    INTO mailing_address_type_id 
    FROM huser.address_type 
    WHERE LOWER(address_type_name) = 'mailing' 
      AND delete_nbr = 0 
    LIMIT 1;

    -- Step 2: Update the delete_nbr field with person_address_id for the migrated records
    IF mailing_address_type_id IS NOT NULL THEN 
        UPDATE huser.person_address 
        SET delete_nbr = person_address_id, -- Set delete_nbr to person_address_id
			is_primary = FALSE -- Set is_primary to false
        WHERE address_type_id = mailing_address_type_id 
          AND source = 'ETL' 
          AND is_primary = TRUE 
          AND delete_nbr = 0; 
    END IF; 
END $$;
