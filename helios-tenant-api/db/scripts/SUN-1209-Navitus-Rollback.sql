-- ============================================================================
-- 🧹 Rollback Script : Remove 'enableStoresSection' from tenant_attr
-- 📌 Purpose         : Undo SUN-1209 changes by removing the key for NAVITUS tenants
-- 👨‍💻 Author          : Riaz
-- 📅 Date            : 2025-11-25
-- 🧾 Jira            : SUN-1209
-- ⚠️ Inputs          : v_tenant_codes[]
-- 📤 Output          : Cleans tenant_attr JSON structure
-- 📝 Notes           :
--    - Removes enableStoresSection key if present.
--    - Idempotent: safe to run several times.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant TEXT;
BEGIN
    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP
        UPDATE tenant.tenant
        SET tenant_attr = tenant_attr - 'enableStoresSection',
            update_ts = NOW()
        WHERE tenant_code = v_tenant
          AND delete_nbr = 0;

        RAISE NOTICE '♻️ Removed enableStoresSection for tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🏁 Rollback complete for enableStoresSection removal.';
END $$;
