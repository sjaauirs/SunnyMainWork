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

-- Ensure every person has exactly one primary phone number.
BEGIN;

DO $$
DECLARE
    v_updated_count INTEGER;
BEGIN
    -- Step 0: Check schema and tables
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.schemata 
        WHERE schema_name = 'huser'
    ) THEN
        RAISE EXCEPTION 'Schema "huser" does not exist.';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'huser' AND table_name = 'person'
    ) THEN
        RAISE EXCEPTION 'Table "huser.person" does not exist.';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'huser' AND table_name = 'phone_number'
    ) THEN
        RAISE EXCEPTION 'Table "huser.phone_number" does not exist.';
    END IF;

    -- Step 1: Rank phone numbers per person
    WITH ranked_phones AS (
        SELECT
            pn.phone_number_id,
            pn.person_id,
            ROW_NUMBER() OVER (
                PARTITION BY pn.person_id
                ORDER BY 
                    CASE 
                        WHEN pn.phone_type_id = 2 THEN 1  -- Mobile 
                        WHEN pn.phone_type_id = 1 THEN 2  -- Home 
                        ELSE 3
                    END,
                    pn.phone_number_id  -- Tie-breaker: lower ID
            ) AS rn
        FROM huser.phone_number pn
        INNER JOIN huser.person p ON pn.person_id = p.person_id
        WHERE pn.delete_nbr = 0
          AND p.delete_nbr = 0
          AND NOT EXISTS (
              SELECT 1
              FROM huser.phone_number x
              WHERE x.person_id = pn.person_id 
                AND x.is_primary = true
                AND x.delete_nbr = 0
          )
    )

    -- Step 2: Update top-ranked phone numbers to is_primary = true
    UPDATE huser.phone_number
    SET is_primary = true
    WHERE phone_number_id IN (
        SELECT phone_number_id
        FROM ranked_phones
        WHERE rn = 1
    );

    -- Get number of rows updated
    GET DIAGNOSTICS v_updated_count = ROW_COUNT;
    RAISE NOTICE 'Updated % phone_number(s) as primary.', v_updated_count;

END $$;

COMMIT;