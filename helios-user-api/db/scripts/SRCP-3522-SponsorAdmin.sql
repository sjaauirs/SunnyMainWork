-- This is a sample script to create a Sponsor Admin user.
-- Steps to get SponsorAdmin code
-- select * from huser.consumer where consumer_code = 'cmr-70ae43efba8e4ce691655d5de16b1fc3' AND delete_nbr = 0;
-- select * from tenant.tenant where tenant_code = 'ten-ecada21e57154928a2bb959e8365b8b4' AND delete_nbr = 0;
-- select * from tenant.sponsor where sponsor_id = 1 AND delete_nbr = 0; 
-- select * from tenant.customer where customer_id = 1 AND delete_nbr = 0;
-- select * from tenant.sponsor where sponsor_code = 'spo-c008f49aa31f4acd9aa6e2114bfb820e' AND delete_nbr = 0;
-- select * from huser.person_role where customer_code ='cus-04c211b4339348509eaa870cdea59600' AND delete_nbr = 0;

DO $$
DECLARE
    v_consumer_code VARCHAR := 'cmr-70ae43efba8e4ce691655d5de16b1fc3';  -- Replace with the actual consumer_code
    v_role_code VARCHAR := 'rol-2b394f0307554d99b106e8fe0518bd04';      -- Replace with the actual role_code
    v_customer_code VARCHAR;                -- To hold the customer_code
    v_sponsor_code VARCHAR;                 -- To hold the sponsor_code
    v_tenant_code VARCHAR := 'ALL';         -- Example tenant_code
    v_create_user VARCHAR := 'SYSTEM';      -- Example create_user
    v_person_id BIGINT;                     -- To hold the fetched person_id
    v_role_id BIGINT;                       -- To hold the fetched role_id
    v_message TEXT;                         -- To hold the message
    v_exists BOOLEAN;                       -- To check if record already exists
BEGIN
    -- Fetch person_id from huser.consumer
    SELECT person_id INTO v_person_id
    FROM huser.consumer
    WHERE consumer_code = v_consumer_code AND delete_nbr = 0;

    IF NOT FOUND THEN
        RAISE NOTICE 'Insert Failed: No person_id found for consumer_code = %', v_consumer_code;
        RETURN;
    END IF;

    -- Fetch role_id from huser.role
    SELECT role_id INTO v_role_id
    FROM huser.role
    WHERE role_code = v_role_code AND delete_nbr = 0;

    IF NOT FOUND THEN
        RAISE NOTICE 'Insert Failed: No role_id found for role_code = %', v_role_code;
        RETURN;
    END IF;

    -- Fetch customer_code and sponsor_code dynamically
    WITH ConsumerTenant AS (
        SELECT tenant_code
        FROM huser.consumer
        WHERE consumer_code = v_consumer_code AND delete_nbr = 0
    ),
    TenantSponsor AS (
        SELECT sponsor_id
        FROM tenant.tenant
        WHERE tenant_code = (SELECT tenant_code FROM ConsumerTenant) AND delete_nbr = 0
    ),
    SponsorCustomer AS (
        SELECT customer_id
        FROM tenant.sponsor
        WHERE sponsor_id = (SELECT sponsor_id FROM TenantSponsor) AND delete_nbr = 0
    )
    SELECT 
        c.customer_code,
        s.sponsor_code
    INTO 
        v_customer_code,
        v_sponsor_code
    FROM tenant.customer c, tenant.sponsor s
    WHERE c.customer_id = (SELECT customer_id FROM SponsorCustomer) AND c.delete_nbr = 0
      AND s.sponsor_id = (SELECT sponsor_id FROM TenantSponsor) AND s.delete_nbr = 0;

    -- Check if the record already exists
    SELECT EXISTS (
        SELECT 1
        FROM huser.person_role
        WHERE person_id = v_person_id
          AND role_id = v_role_id
          AND customer_code = v_customer_code
          AND sponsor_code = v_sponsor_code
          AND tenant_code = v_tenant_code
		  AND delete_nbr = 0
    ) INTO v_exists;

    IF v_exists THEN
        v_message := 'Insert Skipped: Record already exists for person_id = ' || v_person_id || ', role_id = ' || v_role_id;
    ELSE
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
                NOW() AT TIME ZONE 'UTC',
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
    END IF;

    RAISE NOTICE '%', v_message;
END $$;
