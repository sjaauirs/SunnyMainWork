-- 🚀 Script    : Add colour codes to tenant_attr.ux.calendarColors
-- 📌 Purpose   : For HAP-TENANT-CODE update tenant_attr->ux->calendarColors
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-09-29
-- 🧾 Jira      : RES-392
-- ⚠️ Inputs    : HAP-TENANT-CODE 
-- 📤 Output    : It will update tenant_attr->ux with input color codes
-- 🔗 Script URL: NA
-- 📝 Notes     : https://github.com/SunnyRewards/helios-tenant-api/blob/develop/db/scripts/RES-392-HAP-tenant-update-tenant_attr_calendar_colors.sql

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- 🔹 Input tenant_code
    v_new_colors JSONB := '{
        "primaryColor":"#181D27",
        "secondaryColor":"#a4a4a4ff",
        "borderColor":"#C9CACC",
        "backgroundPrimaryColor":"#FFFFFF",
        "backgroundSecondaryColor":"#F5F6F7",
        "disabledTextColor":"#CCC",
        "selectedColor":"#005572"
    }';
    v_exists BOOLEAN;
BEGIN
    -- Check if tenant exists
    IF NOT EXISTS (SELECT 1 FROM tenant.tenant WHERE tenant_code = v_tenant_code) THEN
        RAISE EXCEPTION '❌ Tenant with code "%" not found', v_tenant_code;
    END IF;

    -- Check if ux->calendarColors already exists
    SELECT (tenant_attr -> 'ux' -> 'calendarColors') IS NOT NULL
    INTO v_exists
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code;

    -- Update tenant_attr
    UPDATE tenant.tenant t
    SET tenant_attr =
        CASE
            -- ux and calendarColors exist → merge new colors
            WHEN v_exists THEN
                jsonb_set(
                    t.tenant_attr,
                    '{ux,calendarColors}',
                    (t.tenant_attr -> 'ux' -> 'calendarColors') || v_new_colors
                )
            -- ux exists but calendarColors missing → add calendarColors inside ux
            WHEN t.tenant_attr ? 'ux' THEN
                jsonb_set(
                    t.tenant_attr,
                    '{ux,calendarColors}',
                    v_new_colors,
                    true
                )
            -- ux missing → create ux with calendarColors
            ELSE
                t.tenant_attr || jsonb_build_object(
                    'ux', jsonb_build_object('calendarColors', v_new_colors)
                )
        END
    WHERE t.tenant_code = v_tenant_code;

    -- Log messages
    IF v_exists THEN
        RAISE NOTICE '✅ Updated ux.calendarColors for tenant: %', v_tenant_code;
    ELSE
        RAISE NOTICE '✅ Added ux.calendarColors for tenant: %', v_tenant_code;
    END IF;

    -- Show final result for verification
    RAISE NOTICE '🔎 Final tenant_attr: %',
        (SELECT tenant_attr FROM tenant.tenant WHERE tenant_code = v_tenant_code);

END $$;
