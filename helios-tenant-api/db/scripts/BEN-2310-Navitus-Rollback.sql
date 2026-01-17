
-- ============================================================================
-- 🔄 Script    : Rollback NAVITUS - revert ux.commonColors keys
-- 📌 Purpose   : Revert textColor
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
                '{ux,commonColors}',
                COALESCE(t.tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
                || jsonb_build_object(
                       'textColor', '#0D1C3D'
                   ),
                true
            ),
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        RAISE NOTICE '🔁 Reverted ux.commonColors keys for tenant %', v_tenant_code;
    END LOOP;
END $$;
