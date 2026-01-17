DO $$
DECLARE
    tenantcode TEXT := '<WATCO-TENANT-CODE>';
    earnmaximum NUMERIC := 55; -- Set 55 maximum value
BEGIN
    --Update earn maximum for REWARDS wallet for Watco tenants
    Update wallet.wallet w
    SET earn_maximum = earnmaximum,
        update_ts = now(),
        update_user = 'SYSTEM'
    WHERE 
    wallet_name = 'REWARDS'
    AND w.tenant_code = tenantcode
    AND delete_nbr = 0;
END;
$$;
