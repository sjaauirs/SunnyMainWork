-- 🔄 Script    : Rollback buttonDisableColor under ux.commonColors for multiple tenants
-- 📌 Purpose   : Reverts buttonDisableColor to previous value (#858D9C)
-- 🧑 Author    : Riaz Ahmed
-- 📅 Date      : 2025-11-07
-- 🧾 Jira      : SUN-851
-- ⚠️ Inputs    : v_tenant_codes (TEXT[])
-- 📤 Output    : tenant_attr.ux.commonColors.buttonDisableColor reverted to "#858D9C"
-- 📝 Notes     : Other keys remain intact; safe to rerun for multiple tenants

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_now         TIMESTAMP := NOW();
    v_user        TEXT := 'SYSTEM-ROLLBACK';
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant t
        SET tenant_attr = jsonb_set(
                              tenant_attr::jsonb,
                              '{ux,commonColors}',
                              COALESCE(tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
                              || jsonb_build_object('buttonDisableColor', '#858D9C'),
                              true
                          ),
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        IF FOUND THEN
            RAISE NOTICE '🔁 Reverted ux.commonColors.buttonDisableColor to #858D9C for tenant %', v_tenant_code;
        ELSE
            RAISE NOTICE '⚠️ No active tenant found for %', v_tenant_code;
        END IF;
    END LOOP;
END $$;
