
-- ============================================================================
-- 🚀 Script    : Add UX button styling to Tenant Attribute for NAVITUS
-- 📌 Purpose   : Adds or replaces the "button" object inside "ux" in tenant_attribute JSONB
-- 🧑 Author    : Bhojesh 
-- 📅 Date      : 2025-12-01
-- 🧾 Jira      : SUN-1213
-- ⚠️ Inputs    : <NAVITUS-TENANT-CODE>
-- 📤 Output    : Updated tenant_attribute JSONB with new button styling
-- 🔗 Script URL: <NA>
-- 📝 Notes     : This script assumes the tenant_attribute column is of type JSONB.
--               If "button" already exists, it will be overwritten.

-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];

    v_button_colors JSONB;
    v_tenant_code TEXT;
BEGIN
    -- Correct JSON object creation
    v_button_colors := jsonb_build_object(
        'borderColor', 'transparent',
        'borderWidth', 0,
        'primaryBgColor', '#326F91',
        'primaryTextColor', '#FFFFFF',
        'borderRadius', 16
    );

    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,button}',
            v_button_colors,
            true
        )
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '[INFO] Updated button for tenant: %', v_tenant_code;
    END LOOP;

END $$;