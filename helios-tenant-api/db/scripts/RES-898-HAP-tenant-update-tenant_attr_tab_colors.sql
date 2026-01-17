-- ==================================================================================================
-- 🚀 Script    : Add or Update tabBar colors in tenant_attr.ux.tabBar
-- 📌 Purpose   : For a given tenant_code, update tenant_attr->ux->tabBar with new color settings.
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-10-16
-- 🧾 Jira      : RES-898(sub-task)
-- ⚠️ Inputs    : HAP_TENANT_CODE
-- 📤 Output    : Ensures ux->tabBar exists with specified colors (adds or updates as needed)
-- 🔗 Script URL: NA
-- 📝 Notes     : Safe to execute multiple times — idempotent behavior ensured
-- ==================================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP_TENANT_CODE>'; --  Input tenant code
    v_new_tabbar JSONB := '{
        "backGroundColor": "#FFFFFF",
        "selectedTabBgColor": "#F7F4F0",
        "textColor": "#181D27"
    }';
    v_exists BOOLEAN;
BEGIN
    -- Check if tenant exists
    IF NOT EXISTS (SELECT 1 FROM tenant.tenant WHERE tenant_code = v_tenant_code AND delete_nbr=0) THEN
        RAISE EXCEPTION '❌ Tenant with code "%" not found', v_tenant_code;
    END IF;

    -- Check if ux->tabBar already exists
    SELECT (tenant_attr -> 'ux' -> 'tabBar') IS NOT NULL
    INTO v_exists
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code;

    -- Update tenant_attr JSONB structure safely
    UPDATE tenant.tenant t
    SET tenant_attr =
        CASE
            -- Case 1: ux->tabBar exists → merge or update the given color values
            WHEN v_exists THEN
                jsonb_set(
                    t.tenant_attr,
                    '{ux,tabBar}',
                    (t.tenant_attr -> 'ux' -> 'tabBar') || v_new_tabbar
                )

            -- Case 2: ux exists but tabBar missing → add tabBar under ux
            WHEN t.tenant_attr ? 'ux' THEN
                jsonb_set(
                    t.tenant_attr,
                    '{ux,tabBar}',
                    v_new_tabbar,
                    true
                )

            -- Case 3: ux missing entirely → create ux with tabBar
            ELSE
                t.tenant_attr || jsonb_build_object(
                    'ux', jsonb_build_object('tabBar', v_new_tabbar)
                )
        END
    WHERE t.tenant_code = v_tenant_code;

    -- Log success messages
    IF v_exists THEN
        RAISE NOTICE '✅ Updated ux.tabBar colors for tenant: %', v_tenant_code;
    ELSE
        RAISE NOTICE '✅ Added ux.tabBar colors for tenant: %', v_tenant_code;
    END IF;

END $$;
