-- ============================================================================
-- 🔁 Script    : Rollback UX Trivia & Common Colors in tenant_attr
-- 📌 Purpose   : Restores:
--                  - ux.triviaColors.progressBarFillColor = '#E27025'
--                  - ux.triviaColors.progressBarBgColor   = '#FFC907'
--                  - ux.commonColors.textColor3           = '#0078B3'
-- 👨‍💻 Author    : Riaz
-- 📅 Date      : 2025-12-15
-- 🧾 Jira      : SUN-1213
-- ⚠️ Inputs    : v_tenant_codes[]
-- 📝 Notes     :
--    - Idempotent
--    - Preserves other UX keys
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
                        'progressBarFillColor', '#E27025',
                        'progressBarBgColor',   '#FFC907'
                    ),
                    true
                ),
                '{ux,commonColors}',
                COALESCE(t.tenant_attr #> '{ux,commonColors}', '{}'::jsonb)
                || jsonb_build_object(
                    'textColor3', '#0078B3'
                ),
                true
            ),
            update_ts = NOW()
        WHERE t.tenant_code = v_tenant
          AND t.delete_nbr = 0;
    END LOOP;
END $$;
