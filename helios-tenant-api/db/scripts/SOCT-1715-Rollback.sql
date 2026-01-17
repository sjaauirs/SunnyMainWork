DO $$
DECLARE
    tenantcode TEXT := '<WATCO-TENANT-CODE>';
BEGIN
    --Revert Update tenant attributes isHybridRewards enabled and nonMonetaryOnly disabled for Watco tenant
    UPDATE tenant.tenant t
    SET tenant_attr = jsonb_set(
                    jsonb_set(tenant_attr, '{isHybridRewards}', 'false'::jsonb),
                    '{nonMonetaryOnly}', 'true'::jsonb
                ),
        update_ts = now(),
        update_user = 'SYSTEM'
    WHERE  t.tenant_code = tenantcode
    AND delete_nbr = 0;
END;
$$;
