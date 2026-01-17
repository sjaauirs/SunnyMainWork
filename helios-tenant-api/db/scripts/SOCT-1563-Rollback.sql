DO $$
DECLARE
    tenantCode TEXT := '<KP-TENANT-CODE>'; -- Replace with actual KP tenant_code
    updateUser TEXT := 'SYSTEM';
    updateTs TIMESTAMP := now();
BEGIN
    -- Remove sunnySDKEnabled flag
    UPDATE tenant.tenant
    SET tenant_attr = tenant_attr - 'sunnySDKEnabled',
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr ? 'sunnySDKEnabled'
      AND delete_nbr = 0;
END $$;