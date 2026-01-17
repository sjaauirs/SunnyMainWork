-- Migrate primary address from huser.person to huser.person_address
DO $$
DECLARE
    mailing_address_type_id BIGINT;
BEGIN
    -- Step 1: Get the address_type_id for 'mailing' address type
    SELECT address_type_id
    INTO mailing_address_type_id
    FROM huser.address_type
    WHERE LOWER(address_type_name) = 'mailing'
      AND delete_nbr = 0
    LIMIT 1;

    -- Step 2: If not found, raise an error
    IF mailing_address_type_id IS NULL THEN
        RAISE EXCEPTION 'Address type "mailing" does not exist in huser.address_type.';
    END IF;

    -- Step 3: Insert mailing addresses into person_address table where not already present
    INSERT INTO huser.person_address (
        address_type_id,
        person_id,
        address_label,
        line1,
        line2,
        city,
        state,
        postal_code,
        region,
		country_code,
        country,
        source,
        is_primary,
        create_ts,
        update_ts,
        create_user,
        update_user,
        delete_nbr
    )
    SELECT
        mailing_address_type_id,
        p.person_id,
        'Primary mailing address',
        p.mailing_addr_line_1,
        p.mailing_addr_line_2,
        p.city,
        p.mailing_state,
        p.postal_code,
        p.region,
		p.mailing_country_code,
        p.country,
        'ETL',
        TRUE,
        NOW(),
        NOW(),
        'ETL',
        'ETL',
        0
    FROM huser.person p
    WHERE p.mailing_addr_line_1 IS NOT NULL
	  AND TRIM(mailing_addr_line_1) <> ''
	  AND p.delete_nbr = 0
      AND NOT EXISTS (
          SELECT 1
          FROM huser.person_address pa
          WHERE pa.person_id = p.person_id
            AND pa.address_type_id = mailing_address_type_id
            AND pa.delete_nbr = 0
      );
END $$