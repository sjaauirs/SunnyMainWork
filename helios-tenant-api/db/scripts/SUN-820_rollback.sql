-- ============================================================================
-- 🚀 Script    : Rollback SUN-820 changes
-- 📌 Purpose   : Updates the commonColors.diableButtonBgColor1 to "#868c92",
--                remove commonColors,disableButtonLabelColor1, commonColors,disableButtonBorderColor1
--                 for the given tenant in tenant.tenant.tenant_attr.
-- 🧑 Author    : Pranav
-- 📅 Date      : 11/05/2025
-- 🧾 Jira      : SUN-820
-- ⚠️ Inputs    : KP-TENANT-CODE, HAP-TENANT-CODE, NAVITUS-TENANT-CODE
-- 📤 Output    : tenant_attr updated with new color value
-- 📝 Notes     : Creates/updates the path if missing.
-- ============================================================================
DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY['KP-TENANT-CODE', 'HAP-TENANT-CODE', 'NAVITUS-TENANT-CODE'];
    v_tenant_code  TEXT;
    v_now          TIMESTAMP := NOW();
    v_user         TEXT := 'SYSTEM';
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant t
        SET tenant_attr = jsonb_set(
                              (tenant_attr::jsonb
                                  #- '{ux,commonColors,disableButtonLabelColor1}'
                                  #- '{ux,commonColors,disableButtonBorderColor1}'),
                              '{ux,commonColors,disableButtonBgColor1}',
                              to_jsonb('#868c92'),
                              true
                          ),
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        RAISE NOTICE '↩️ Rolled back ux.commonColors for tenant % (Bg=%, Label/Border removed)',
            v_tenant_code, '#868c92';
    END LOOP;
END $$;
