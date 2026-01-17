-- ============================================================================
-- 🔄 Script    : Rollback NAVITUS - revert ux.themeColors and remove ux.button.primary keys
-- 📌 Purpose   : Revert headerBgColor,taskGradient1,taskGradient2,entriesGradient1,entriesGradient2 and rActivityTextColor
-- 🧑 Author    : Bhojesh
-- 📅 Date      : 2025-11-13
-- ============================================================================

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
            jsonb_set(
                COALESCE(t.tenant_attr::jsonb, '{}'::jsonb),
                '{ux,themeColors}',
                COALESCE(t.tenant_attr::jsonb #> '{ux,themeColors}', '{}'::jsonb)
                || jsonb_build_object(
                        'headerBgColor', '#0D1C3D',
                        'taskGradient1', '#0078b3',
                        'taskGradient2', '#0D1C3D',
                        'entriesGradient1','#0078b3',
                        'entriesGradient2','#0D1C3D',
                        'rActivityTextColor','#0D1C3D'
                ),
                true
            )
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        RAISE NOTICE '🔁 Reverted ux.themeColors.headerBgColor, taskGradient1, taskGradient2, entriesGradient1, entriesGradient2 and rActivityTextColors keys for tenant %', v_tenant_code;
    END LOOP;
END $$;
 