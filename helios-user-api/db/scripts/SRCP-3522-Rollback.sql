-- Script to update delete_nbr, update_ts, and update_user for multiple role_codes.

DO $$
DECLARE
    v_consumer_code VARCHAR := 'cmr-70ae43efba8e4ce691655d5de16b1fc3';  -- Example consumer_code
    v_role_codes VARCHAR[] := ARRAY[
        'rol-d2ad5b10f47940eaa1e190387433e64c',
        'rol-46c2740cafc44869a8b1f822bf5fa712',
        'rol-938b67672b3f4dcaba54435e95dda6c5',
        'rol-2b394f0307554d99b106e8fe0518bd04',
        'rol-44e107f38dda4423bf653525fb9c1938',
        'rol-f7ca3ef923614892907094230365bb82'
    ];                                                                  -- Array of role_codes
    v_update_user VARCHAR := 'SYSTEM';                                  -- Example update_user
    v_person_id BIGINT;                                                 -- To hold the fetched person_id
    v_role_id BIGINT;                                                   -- To hold the fetched role_id
    v_message TEXT;                                                     -- To hold the message
    v_role_code VARCHAR;                                                -- To hold current role_code
BEGIN
    -- Fetch person_id from huser.consumer
    SELECT person_id INTO v_person_id
    FROM huser.consumer
    WHERE consumer_code = v_consumer_code;

    IF NOT FOUND THEN
        RAISE NOTICE 'Update Failed: No person_id found for consumer_code = %', v_consumer_code;
        RETURN;
    END IF;

    -- Loop through each role_code in the array
    FOR v_role_code IN SELECT UNNEST(v_role_codes) LOOP
        -- Fetch role_id from huser.role
        SELECT role_id INTO v_role_id
        FROM huser.role
        WHERE role_code = v_role_code;

        IF NOT FOUND THEN
            RAISE NOTICE 'Update Skipped: No role_id found for role_code = %', v_role_code;
            CONTINUE; -- Skip to the next role_code
        END IF;

        BEGIN
            UPDATE huser.person_role
            SET 
                delete_nbr = person_role_id,
                update_ts = NOW() AT TIME ZONE 'UTC',
                update_user = v_update_user
            WHERE 
                person_id = v_person_id
                AND role_id = v_role_id;

            IF NOT FOUND THEN
                v_message := 'Update Failed: No matching record found for person_id = ' || v_person_id || ', role_id = ' || v_role_id || ', role_code = ' || v_role_code;
            ELSE
                v_message := 'Update Success: delete_nbr updated to person_role_id for person_id = ' || v_person_id || ', role_id = ' || v_role_id || ', role_code = ' || v_role_code;
            END IF;

        EXCEPTION
            WHEN OTHERS THEN
                v_message := 'Update Failed for role_code ' || v_role_code || ': ' || SQLERRM;
        END;

        RAISE NOTICE '%', v_message;
    END LOOP;
END $$;