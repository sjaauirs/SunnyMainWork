-- ============================================================================
-- 🚀 Script    : Update ux.commonColors
-- 📌 Purpose   : Updates the commonColors.diableButtonBgColor1 to "#D3D6DC",
--                 commonColors.disableButtonLabelColor1 to "#858D9C"
--                 for the given tenant in tenant.tenant.tenant_attr.
-- 🧑 Author    : Pranav
-- 📅 Date      : 11/05/2025
-- 🧾 Jira      : SUN-820
-- ⚠️ Inputs    : KP-TENANT-CODE
-- 📤 Output    : tenant_attr updated with new color value
-- 📝 Notes     : Creates/updates the path if missing.
-- ============================================================================
DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY['KP-TENANT-CODE'];
    v_tenant_code  TEXT;
    v_now          TIMESTAMP := NOW();
    v_user         TEXT := 'SYSTEM';
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant t
        SET tenant_attr = jsonb_set(
                              tenant_attr::jsonb,
                              '{ux,commonColors}',
                              COALESCE(tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
                              || jsonb_build_object(
                                     'disableButtonBgColor1',    '#D3D6DC',
                                     'disableButtonLabelColor1', '#858D9C'
                                 ),
                              true
                          ),
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        RAISE NOTICE '✅ Updated ux.commonColors for tenant % (Bg=%, Label=%)',
            v_tenant_code, '#D3D6DC', '#858D9C';
    END LOOP;
END $$;
-- ============================================================================
-- 🚀 Script    : Update ux.commonColors
-- 📌 Purpose   : Updates the commonColors.diableButtonBgColor1 to "#C9CACC",
--                 commonColors.disableButtonLabelColor1 to "#ABAEB2",
--                 commonColors.diableButtonBgColor1 to "#909399"
--                 for the given tenant in tenant.tenant.tenant_attr.
-- 🧑 Author    : Pranav
-- 📅 Date      : 11/05/2025
-- 🧾 Jira      : SUN-820
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : tenant_attr updated with new color value
-- 📝 Notes     : Creates/updates the path if missing.
-- ============================================================================
DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY['HAP-TENANT-CODE'];
    v_tenant_code  TEXT;
    v_now          TIMESTAMP := NOW();
    v_user         TEXT := 'SYSTEM';
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant t
        SET tenant_attr = jsonb_set(
                              tenant_attr::jsonb,
                              '{ux,commonColors}',
                              COALESCE(tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
                              || jsonb_build_object(
                                     'disableButtonBgColor1',    '#C9CACC',
                                     'disableButtonLabelColor1', '#ABAEB2',
                                     'disableButtonBorderColor1','#909399'
                                 ),
                              true
                          ),
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        RAISE NOTICE '✅ Updated ux.commonColors for tenant % (Bg=%, Label=%, Border=%)',
            v_tenant_code, '#C9CACC', '#ABAEB2', '#909399';
    END LOOP;
END $$;
-- ============================================================================
-- 🚀 Script    : Update ux.commonColors
-- 📌 Purpose   : Updates the commonColors.diableButtonBgColor1 to "#E3E5E8",
--                 commonColors.disableButtonLabelColor1 to "#5F6062"
--                 for the given tenant in tenant.tenant.tenant_attr.
-- 🧑 Author    : Pranav
-- 📅 Date      : 11/05/2025
-- 🧾 Jira      : SUN-820
-- ⚠️ Inputs    : NAVITUS-TENANT-CODE
-- 📤 Output    : tenant_attr updated with new color value
-- 📝 Notes     : Creates/updates the path if missing.
-- ============================================================================
DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY['NAVITUS-TENANT-CODE'];
    v_tenant_code  TEXT;
    v_now          TIMESTAMP := NOW();
    v_user         TEXT := 'SYSTEM';
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant t
        SET tenant_attr = jsonb_set(
                              tenant_attr::jsonb,
                              '{ux,commonColors}',
                              COALESCE(tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
                              || jsonb_build_object(
                                     'disableButtonBgColor1',    '#E3E5E8',
                                     'disableButtonLabelColor1', '#5F6062'
                                 ),
                              true
                          ),
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        RAISE NOTICE '✅ Updated ux.commonColors for tenant % (Bg=%, Label=%)',
            v_tenant_code, '#E3E5E8', '#5F6062';
    END LOOP;
END $$;

