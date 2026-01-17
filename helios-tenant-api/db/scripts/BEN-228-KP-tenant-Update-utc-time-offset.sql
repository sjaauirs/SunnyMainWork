DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';  -- Replace with input tenant_code
    v_utc_time_offset TEXT := 'UTC-08:00';        -- KP Tenant ("UTC-08:00" => "Pacific Standard Time",)
    v_count INT;
BEGIN
    BEGIN
        -- Check if the tenant exists
        SELECT COUNT(*) INTO v_count
        FROM tenant.tenant
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        IF v_count > 0 THEN
            -- Update the value
            UPDATE tenant.tenant
            SET utc_time_offset = v_utc_time_offset,
				update_ts = NOW(),
				update_user = 'SYSTEM'
            WHERE tenant_code = v_tenant_code
              AND delete_nbr = 0;

            RAISE NOTICE '✅ Updated UTC time offset to % for tenant %', v_utc_time_offset, v_tenant_code;
        ELSE
            -- Tenant not found
            RAISE NOTICE '⚠️ Tenant not found with tenant_code: %', v_tenant_code;
        END IF;
    END;
END $$;

