-- 🔄 Script    : Rollback activateButtonColor under ux.mycardColors for KP tenant
-- 📌 Purpose   : Reverts activateButtonColor to previous value (#57A635)
-- 🧑 Author    : Riaz Ahmed
-- 📅 Date      : 2025-10-31
-- 🧾 Jira      : SUN-867
-- ⚠️ Inputs    : v_tenant_code 
-- 📤 Output    : tenant_attr.ux.mycardColors.activateButtonColor reverted to "#57A635"
-- 📝 Notes     : Other keys remain intact; only KP tenant ux, mycardColors impacted

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
                              '{ux,mycardColors}',
                              COALESCE(tenant_attr::jsonb #> '{ux,mycardColors}', '{}'::jsonb)
                              || jsonb_build_object('activateButtonColor', '#57A635'),
                              true
                          ),
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        RAISE NOTICE '🔁 Reverted ux.mycardColors.activateButtonColor to #57A635 for tenant %', v_tenant_code;
    END LOOP;
END $$;