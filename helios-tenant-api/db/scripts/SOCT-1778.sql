DO
$$
DECLARE
	v_row_count INT;
    v_user_id TEXT := 'SYSTEM';
    v_now TIMESTAMP := NOW();
    -- List of tenants where flags should be TRUE
    v_target_tenant_codes TEXT[] := ARRAY[
        '<KP-TENANT-CODE>'  -- add more tenant codes here
    ];
BEGIN
    -- 1. Update target tenants (flags = true)
    UPDATE tenant.tenant
    SET tenant_attr = COALESCE(tenant_attr, '{}'::JSONB)
        || jsonb_build_object(
            'displayBancorpCopyright', true
        ),
        update_user = v_user_id,
        update_ts = v_now
    WHERE tenant_code = ANY(v_target_tenant_codes)
      AND delete_nbr = 0;

	GET DIAGNOSTICS v_row_count = ROW_COUNT;
	RAISE NOTICE 'Updated target tenants to TRUE flags. Rows affected: %', v_row_count;
	
    -- 2. Update all other active tenants (flags = false)
    UPDATE tenant.tenant
    SET tenant_attr = COALESCE(tenant_attr, '{}'::JSONB)
        || jsonb_build_object(
            'displayBancorpCopyright', false
        ),
        update_user = v_user_id,
        update_ts = v_now
    WHERE tenant_code <> ALL(v_target_tenant_codes)
      AND delete_nbr = 0;

	GET DIAGNOSTICS v_row_count = ROW_COUNT;
	RAISE NOTICE 'Updated non-target tenants to FALSE flags. Rows affected: %', v_row_count;
END
$$;