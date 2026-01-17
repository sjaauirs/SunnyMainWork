-- 🔄 Script    : Rollback button colors under ux.commonColors and ux.button for NAVITUS tenant
-- 📌 Purpose   : Reverts ux.commonColors to previous values and removes ux.button.primary color keys
-- 🧑 Author    : Riaz Ahmed
-- 📅 Date      : 2025-11-12
-- 🧾 Jira      : 
-- ⚠️ Inputs    : v_tenant_codes (TEXT[])
-- 📤 Output    : tenant_attr.ux.commonColors.button1Color, tenant_attr.ux.commonColors.button1TextColor reverted,
--                ux.button.primaryBgColor and ux.button.primaryTextColor removed
-- 📝 Notes     : Safe to rerun; only modifies specified keys

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_now         TIMESTAMP := NOW();
    v_user        TEXT := 'SYSTEM-ROLLBACK';
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant t
        SET tenant_attr =
            -- 🔁 Revert ux.commonColors.button1Color and button1TextColor
            jsonb_set(
                tenant_attr::jsonb,
                '{ux,commonColors}',
                COALESCE(tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
                || jsonb_build_object(
                    'button1Color', '#0078b3',
                    'button1TextColor', '#FFFFFF'
                ),
                true
            )
            -- 🔁 Remove ux.button.primaryBgColor and primaryTextColor
            #- '{ux,button,primaryBgColor}'
            #- '{ux,button,primaryTextColor}',
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        IF FOUND THEN
            RAISE NOTICE '🔁 Reverted ux.commonColors and removed ux.button.primary color keys for tenant %', v_tenant_code;
        ELSE
            RAISE NOTICE '⚠️ No active tenant found for %', v_tenant_code;
        END IF;
    END LOOP;
END $$;
