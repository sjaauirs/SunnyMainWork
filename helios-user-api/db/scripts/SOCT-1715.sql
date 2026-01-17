DO $$
DECLARE
    tenantcode TEXT := '<WATCO-TENANT-CODE>';
BEGIN

    -- Update SSO user flag for Watco tenants
    UPDATE huser.consumer c
    SET
        is_sso_user = true,
        update_ts = now(),
        update_user = 'SYSTEM'
    WHERE
        c.tenant_code = tenantcode
        AND c.delete_nbr = 0;
END;
$$;
