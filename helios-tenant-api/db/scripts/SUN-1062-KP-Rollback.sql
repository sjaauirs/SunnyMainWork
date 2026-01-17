-- ============================================================================
-- 🚀 Script    : Rollbacks colors under ux.commonColors and ux.themeColors for KP tenant
-- 📌 Purpose   : Rollbacks errorBorderColor, dollar and RewardDialTextColor
-- 🧑 Author    : Riaz Ahmed
-- 📅 Date      : 2025-11-21
-- 🧾 Jira      : SUN-1062
-- ⚠️ Inputs    : v_tenant_code 
-- 📤 Output    : tenant_attr.ux.commonColors and ux.themeColors updated
-- 📝 Notes     : Safe to rerun; existing keys remain intact
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_now         TIMESTAMP := NOW();
    v_user        TEXT := 'SYSTEM';
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant t
        SET tenant_attr =
            jsonb_set(                                         
                jsonb_set(                                          
                    COALESCE(t.tenant_attr::jsonb, '{}'::jsonb),
                    '{ux,commonColors}',
                    COALESCE(t.tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
                    || jsonb_build_object(
                        'errorBorderColor', '#8A210B',
                        'dollar', '#003B71'
                    ),
                    true
                ),
                '{ux,themeColors}',
                COALESCE(t.tenant_attr::jsonb #> '{ux,themeColors}', '{}'::jsonb)
                || jsonb_build_object(
                    'RewardDialTextColor',   '#003B71'
                ),
                true
            ),
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;
 
        RAISE NOTICE '✅ Rollbacks ux.commonColors.errorBorderColor, dollar and ux.themeColors.RewardDialTextColor for tenant %', v_tenant_code;
    END LOOP;
END $$;