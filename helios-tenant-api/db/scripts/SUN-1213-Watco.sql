-- ============================================================================
-- 🚀 Script    : Update UX Trivia & Common Colors in tenant_attr
-- 📌 Purpose   : Updates:
--                  - ux.triviaColors.progressBarFillColor
--                  - ux.triviaColors.progressBarBgColor
--                  - ux.commonColors.textColor3
--                  - ux.commonColors.screenTitleShadowColor
-- 👨‍💻 Author    : Riaz
-- 📅 Date      : 2025-12-15
-- 🧾 Jira      : SUN-1213
-- ⚠️ Inputs    : v_tenant_codes[]
-- 📝 Notes     :
--    - Idempotent. Safe to rerun.
--    - Safely initializes missing JSON paths
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'WATCO-TENANT-CODE'
    ];
    v_tenant TEXT;
BEGIN
    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP
        UPDATE tenant.tenant t
        SET tenant_attr =
            jsonb_set(
                jsonb_set(
                    COALESCE(t.tenant_attr, '{}'::jsonb),
                    '{ux,triviaColors}',
                    COALESCE(t.tenant_attr #> '{ux,triviaColors}', '{}'::jsonb)
                    || jsonb_build_object(
                        'progressBarFillColor', '#46798C',
                        'progressBarBgColor',   '#D3D6DC'
                    ),
                    true
                ),
                '{ux,commonColors}',
                COALESCE(t.tenant_attr #> '{ux,commonColors}', '{}'::jsonb)
                || jsonb_build_object(
                    'textColor3',             '#327B8F',
                    'screenTitleShadowColor', '#E3E5E8'
                ),
                true
            ),
            update_ts = NOW()
        WHERE t.tenant_code = v_tenant
          AND t.delete_nbr = 0;
    END LOOP;
END $$;