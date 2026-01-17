
-- ============================================================================
-- 🚀 Script    : Add UX disable button styling to Tenant Attribute for HAP
-- 📌 Purpose   : Adds or replaces the "disableButton" object inside "ux" in tenant_attribute JSONB
-- 🧑 Author    : Bhojesh 
-- 📅 Date      : 2025-11-27
-- 🧾 Jira      : RES-1368
-- ⚠️ Inputs    : <HAP-TENANT-CODE>
-- 📤 Output    : Updated tenant_attribute JSONB with new disableButton styling
-- 🔗 Script URL: <NA>
-- 📝 Notes     : This script assumes the tenant_attribute column is of type JSONB.
--               If "disableButton" already exists, it will be overwritten.

-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'HAP-TENANT-CODE'
    ];
    v_disable_button_colors JSONB;
    v_tenant_code TEXT;
BEGIN
    -- Build JSON using jsonb_build_object (supports int + text)
    v_disable_button_colors := jsonb_build_object(
        'borderWidth', 0,
        'primaryTextColor', '#5C5F66',
        'primaryBgColor', '#E3E4E5',
        'borderColor', 'transparent'
    );

    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux,disableButton}',
            v_disable_button_colors,
            true
        )
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '[INFO] Updated disableButton for tenant: %', v_tenant_code;
    END LOOP;
END $$;