-- Rollback update tenant_attr for Watco and Sunny tenant
DO $$
DECLARE
    tenantCode TEXT := '<WATCO/SUNNY-TENANT-CODE>'; -- Replace with actual code
    updateUser TEXT := 'SYSTEM';
    updateTs TIMESTAMP := now();
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr = (
            jsonb_set(
                jsonb_set(
                    tenant_attr
                        - '{ux,themeColors,entriesGradient1}'
                        - '{ux,themeColors,entriesGradient2}'
                        - '{ux,themeColors,taskGradient1}'
                        - '{ux,themeColors,taskGradient2}'
                        - '{ux,themeColors,rActivityTextColor}'
                        - '{ux,commonColors}',
                    '{ux,themeColors,tileLinear1Color}',
                    '"#003B71"',
                    true
                ),
                '{ux,themeColors,headerBgColor}',
                '"#0D1C3D"',
                true
            )
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;
END $$;


-- Rollback update tenant_attr for Navitus tenant
DO $$
DECLARE
    tenantCode TEXT := '<NAVITUS-TENANT-CODE>'; -- Replace with actual code
    updateUser TEXT := 'SYSTEM';
    updateTs TIMESTAMP := now();
BEGIN
    -- Remove added theme color keys
    UPDATE tenant.tenant
    SET tenant_attr = tenant_attr
        - '{ux,themeColors,entriesGradient1}'
        - '{ux,themeColors,entriesGradient2}'
        - '{ux,themeColors,taskGradient1}'
        - '{ux,themeColors,taskGradient2}'
        - '{ux,themeColors,rActivityTextColor}',
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Restore activeTabBgColor to original
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,taskTileColors,activeTabBgColor}',
            '"#5F6062"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;
END $$;


-- Rollback update tenant_attr for KP tenant
DO $$
DECLARE
    tenantCode TEXT := '<KP-TENANT-CODE>'; -- Replace with actual code
    updateUser TEXT := 'SYSTEM';
    updateTs TIMESTAMP := now();
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr = tenant_attr
        - '{ux,themeColors,entriesGradient1}'
        - '{ux,themeColors,entriesGradient2}'
        - '{ux,themeColors,taskGradient1}'
        - '{ux,themeColors,taskGradient2}'
        - '{ux,themeColors,rActivityTextColor}',
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;
END $$;