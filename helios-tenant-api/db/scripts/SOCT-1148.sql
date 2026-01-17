-- Update tenant_attr for Watco and Sunny tenants
DO $$
DECLARE
    tenantCode TEXT := '<WATCO/SUNNY-TENANT-CODE>'; -- Replace with actual tenant code
    updateUser TEXT := 'SYSTEM';
    updateTs TIMESTAMP := now();
BEGIN
    -- Update headerBgColor
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr::jsonb,
            '{ux,themeColors,headerBgColor}',
            '"#000000"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr <> '{}'::jsonb
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Update tileLinear1Color
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr::jsonb,
            '{ux,themeColors,tileLinear1Color}',
            '"#000000"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr <> '{}'::jsonb
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add entriesGradient1
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr::jsonb,
            '{ux,themeColors,entriesGradient1}',
            '"#0078b3"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add entriesGradient2
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr::jsonb,
            '{ux,themeColors,entriesGradient2}',
            '"#0D1C3D"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add taskGradient1
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr::jsonb,
            '{ux,themeColors,taskGradient1}',
            '"#0078b3"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add taskGradient2
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr::jsonb,
            '{ux,themeColors,taskGradient2}',
            '"#0D1C3D"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add rActivityTextColor
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr::jsonb,
            '{ux,themeColors,rActivityTextColor}',
            '"#000000"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add commonColors object (as a block insert)
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr::jsonb,
            '{ux,commonColors}',
            '{
                "button1Color": "#FFC907",
                "button1TextColor": "#000000",
                "paginationDotActiveColor": "#0D1C3D",
                "paginationDotNonActiveColor": "#D3D6DC"
            }'::jsonb,
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;
END $$;


-- Update tenant_attr for Navitus tenant
DO $$
DECLARE
    tenantCode TEXT := '<NAVITUS-TENANT-CODE>'; -- Replace with actual tenant code
    updateUser TEXT := 'SYSTEM';
    updateTs TIMESTAMP := now();
BEGIN
    -- Add entriesGradient1
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,themeColors,entriesGradient1}',
            '"#0078b3"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add entriesGradient2
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,themeColors,entriesGradient2}',
            '"#0D1C3D"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add taskGradient1
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,themeColors,taskGradient1}',
            '"#0078b3"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add taskGradient2
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,themeColors,taskGradient2}',
            '"#0D1C3D"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add rActivityTextColor
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,themeColors,rActivityTextColor}',
            '"#0D1C3D"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Update activeTabBgColor
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,taskTileColors,activeTabBgColor}',
            '"#0078B3"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;
END $$;


-- Update tenant_attr for KP tenant
DO $$
DECLARE
    tenantCode TEXT := '<KP-TENANT-CODE>'; -- Replace with actual tenant code
    updateUser TEXT := 'SYSTEM';
    updateTs TIMESTAMP := now();
BEGIN
    -- Add entriesGradient1
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,themeColors,entriesGradient1}',
            '"#0078b3"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add entriesGradient2
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,themeColors,entriesGradient2}',
            '"#0D1C3D"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add taskGradient1
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,themeColors,taskGradient1}',
            '"#0078b3"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add taskGradient2
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,themeColors,taskGradient2}',
            '"#0078b3"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;

    -- Add rActivityTextColor
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,themeColors,rActivityTextColor}',
            '"#0D1C3D"',
            true
        ),
        update_user = updateUser,
        update_ts = updateTs
    WHERE tenant_code = tenantCode
      AND tenant_attr IS NOT NULL
      AND tenant_attr::jsonb ? 'ux'
      AND delete_nbr = 0;
END $$;