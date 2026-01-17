DO $$
DECLARE
    v_role_code CONSTANT VARCHAR := 'rol-46c2740cafc44869a8b1f822bf5fa712'; -- Subscriber role_code
    rec RECORD;
    v_message TEXT;
BEGIN
    -- Loop through all subscriber role records
    FOR rec IN 
        SELECT pr.person_id, pr.role_id
        FROM huser.person_role pr
        JOIN huser.role r ON pr.role_id = r.role_id
        WHERE r.role_code = v_role_code 
          AND pr.delete_nbr = 0
          AND (pr.tenant_code IS NULL OR pr.sponsor_code IS NULL OR pr.customer_code IS NULL) -- Condition to process only missing values
    LOOP
        /*
            Fetch updated tenant_code, sponsor_code, and customer_code dynamically
            using Common Table Expressions (CTEs) for better readability and efficiency.
        */
        WITH 
        -- Get tenant_code from consumer table based on person_id
        ConsumerTenant AS (
            SELECT tenant_code 
            FROM huser.consumer 
            WHERE person_id = rec.person_id AND delete_nbr = 0
        ),
        
        -- Get sponsor_code and sponsor_id from sponsor table based on tenant_code
        TenantSponsor AS (
            SELECT sponsor_code, sponsor_id 
            FROM tenant.sponsor 
            WHERE sponsor_id = (
                SELECT sponsor_id 
                FROM tenant.tenant 
                WHERE tenant_code = (SELECT tenant_code FROM ConsumerTenant) 
                AND delete_nbr = 0
            ) 
            AND delete_nbr = 0
        ),
        
        -- Get customer_code from customer table based on sponsor_id
        SponsorCustomer AS (
            SELECT customer_code 
            FROM tenant.customer 
            WHERE customer_id = (
                SELECT customer_id 
                FROM tenant.sponsor 
                WHERE sponsor_id = (SELECT sponsor_id FROM TenantSponsor) 
                AND delete_nbr = 0
            ) 
            AND delete_nbr = 0
        )
        
        -- Update the person_role table with new tenant_code, sponsor_code, and customer_code
        UPDATE huser.person_role
        SET 
            tenant_code  = (SELECT tenant_code FROM ConsumerTenant),
            sponsor_code = (SELECT sponsor_code FROM TenantSponsor),
            customer_code = (SELECT customer_code FROM SponsorCustomer),
            update_ts = NOW() AT TIME ZONE 'UTC',  -- Update timestamp
            update_user = 'SYSTEM'  -- Mark update source
        WHERE person_id = rec.person_id 
          AND role_id = rec.role_id;

        -- Log success message
        v_message := 'Update Success: Updated person_id = ' || rec.person_id;
        RAISE NOTICE '%', v_message;
    END LOOP;
END $$;
