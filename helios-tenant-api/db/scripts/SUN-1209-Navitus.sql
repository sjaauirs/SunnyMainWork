-- ============================================================================
-- 🚀 Script    : Add 'enableStoresSection' to tenant_attr
-- 📌 Purpose   : Set enableStoresSection = true for all NAVITUS tenants
-- 👨‍💻 Author    : Riaz
-- 📅 Date      : 2025-11-25
-- 🧾 Jira      : SUN-1209
-- ⚠️ Inputs    : v_tenant_codes[]
-- 📤 Output    : Updates tenant_attr JSON structure
-- 📝 Notes     :
--    - Adds or overwrites enableStoresSection in tenant_attr.
--    - Idempotent: safe to run multiple times.
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
        SET tenant_attr = jsonb_set(
                tenant_attr,
                '{enableStoresSection}',
                'true'::jsonb,
                true
            ),
            update_ts = NOW()
        WHERE tenant_code = v_tenant
          AND delete_nbr = 0;

        RAISE NOTICE '✅ enableStoresSection set to true for tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🏁 Completed updating enableStoresSection for all NAVITUS tenants.';
END $$;