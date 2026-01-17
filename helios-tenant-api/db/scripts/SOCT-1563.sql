DO $$
DECLARE
    tenantCode TEXT := '<KP-TENANT-CODE>'; -- Replace with actual KP tenant_code
    updateUser TEXT := 'SYSTEM';
    updateTs TIMESTAMP := now();
BEGIN
    -- Add sunnySDKEnabled: true at the root level of tenant_attr
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{sunnySDKEnabled}',
            'true',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND delete_nbr = 0;
END $$;