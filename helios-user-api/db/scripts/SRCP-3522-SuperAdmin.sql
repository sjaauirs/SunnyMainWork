-- This is a sample script to create a Super Admin user.

DO $$
DECLARE
    v_consumer_code VARCHAR := 'cmr-70ae43efba8e4ce691655d5de16b1fc3';  -- Example consumer_code
    v_role_code VARCHAR := 'rol-d2ad5b10f47940eaa1e190387433e64c';		-- Replace with the actual role_code
    v_customer_code VARCHAR := 'ALL';		-- Example customer_code
    v_sponsor_code VARCHAR := 'ALL';		-- Example sponsor_code
    v_tenant_code VARCHAR := 'ALL';			-- Example tenant_code
	v_create_user VARCHAR := 'SYSTEM';	-- Example create_user
	v_person_id BIGINT;	-- To hold the fetched person_id
    v_role_id BIGINT;	-- To hold the fetched role_id
    v_message TEXT;	-- To hold the message
BEGIN
    -- Fetch person_id from huser.consumer
    SELECT person_id INTO v_person_id
    FROM huser.consumer
    WHERE consumer_code = v_consumer_code;

    IF NOT FOUND THEN
        RAISE NOTICE 'Insert Failed: No person_id found for consumer_code = %', v_consumer_code;
        RETURN;
    END IF;

    -- Fetch role_id from huser.role
    SELECT role_id INTO v_role_id
    FROM huser.role
    WHERE role_code = v_role_code;

    IF NOT FOUND THEN
        RAISE NOTICE 'Insert Failed: No role_id found for role_code = %', v_role_code;
        RETURN;
    END IF;

    BEGIN
        INSERT INTO huser.person_role (
            person_id,
            role_id,
            create_ts,
            update_ts,
            create_user,
            update_user,
            delete_nbr,
            customer_code,
            sponsor_code,
            tenant_code
        ) VALUES (
            v_person_id,
            v_role_id,
            CURRENT_TIMESTAMP,
            NULL,
            v_create_user,
            NULL,
            0,
            v_customer_code,
            v_sponsor_code,
            v_tenant_code
        );
        v_message := 'Insert Success: Record added for person_id = ' || v_person_id || ', role_id = ' || v_role_id;
    EXCEPTION
        WHEN OTHERS THEN
            v_message := 'Insert Failed: ' || SQLERRM;
    END;

    RAISE NOTICE '%', v_message;
END $$;
