-- 🚀 Script    : Update button colors under ux.commonColors and ux.button for NAVITUS tenant
-- 📌 Purpose   : Updates primary button and text colors to match NAVITUS brand
-- 🧑 Author    : Riaz Ahmed
-- 📅 Date      : 2025-11-12
-- 🧾 Jira      : 
-- ⚠️ Inputs    : v_tenant_code 
-- 📤 Output    : tenant_attr.ux.commonColors.button1Color, tenant_attr.ux.commonColors.button1TextColor,
--                tenant_attr.ux.button.primaryBgColor, tenant_attr.ux.button.primaryTextColor updated
-- 📝 Notes     : Safe to rerun; existing keys remain intact

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_now         TIMESTAMP := NOW();
    v_user        TEXT := 'SYSTEM';
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant t
        SET tenant_attr = tenant_attr::jsonb
            -- 🔹 Update ux.commonColors.button1Color and button1TextColor
            |> jsonb_set(
                tenant_attr::jsonb,
                '{ux,commonColors}',
                COALESCE(tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
                || jsonb_build_object(
                    'button1Color', '#E27025',
                    'button1TextColor', '#FFFFFF'
                ),
                true
            )
            -- 🔹 Update ux.button.primaryBgColor and primaryTextColor
            |> jsonb_set(
                tenant_attr::jsonb,
                '{ux,button}',
                COALESCE(tenant_attr::jsonb #> '{ux,button}', '{}'::jsonb)
                || jsonb_build_object(
                    'primaryBgColor', '#E27025',
                    'primaryTextColor', '#FFFFFF'
                ),
                true
            ),
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        RAISE NOTICE '✅ Updated ux.commonColors.button1Color, button1TextColor and ux.button.primary colors for tenant %', v_tenant_code;
    END LOOP;
END $$;
