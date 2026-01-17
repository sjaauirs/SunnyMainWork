-- ============================================================================
-- 🔄 Rollback Script : Remove 'hideRewardsGiftBoxIcon' from tenant_attr
-- 📌 Purpose         : Deletes the hideRewardsGiftBoxIcon flag for KP tenants
-- 👨‍💻 Author          : Riaz
-- 📅 Date            : 2025-11-19
-- 🧾 Jira            : SUN-1057
-- ⚠️ Inputs          : v_tenant_codes[]
-- 📤 Output          : Removes key from tenant_attr JSON
-- 📝 Notes           :
--    - Safe & idempotent: no error if key does not exist.
--    - Only affects KP tenants defined in v_tenant_codes.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];

    v_tenant TEXT;
BEGIN
    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP

        UPDATE tenant.tenant
        SET tenant_attr = tenant_attr - 'hideRewardsGiftBoxIcon',
            update_ts = NOW()
        WHERE tenant_code = v_tenant
          AND delete_nbr = 0;

        RAISE NOTICE '🗑 Removed hideRewardsGiftBoxIcon from tenant_attr for tenant: %', v_tenant;

    END LOOP;

    RAISE NOTICE '🏁 Rollback complete: hideRewardsGiftBoxIcon removed for all KP tenants.';
END $$;
