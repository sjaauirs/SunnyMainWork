-- ============================================================================
-- 🚀 Script    : Add 'hideRewardsGiftBoxIcon' to tenant_attr
-- 📌 Purpose   : Set hideRewardsGiftBoxIcon = true for all KP tenants
-- 👨‍💻 Author    : Riaz
-- 📅 Date      : 2025-11-19
-- 🧾 Jira      : SUN-1057
-- ⚠️ Inputs    : v_tenant_codes[]
-- 📤 Output    : Updates tenant_attr JSON structure
-- 📝 Notes     :
--    - Adds or overwrites hideRewardsGiftBoxIcon in tenant_attr.
--    - Idempotent: safe to run multiple times.
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
        SET tenant_attr = jsonb_set(
                tenant_attr,
                '{hideRewardsGiftBoxIcon}',
                'true'::jsonb,
                true
            ),
            update_ts = NOW()
        WHERE tenant_code = v_tenant
          AND delete_nbr = 0;

        RAISE NOTICE '✅ hideRewardsGiftBoxIcon set to true for tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🏁 Completed updating hideRewardsGiftBoxIcon for all KP tenants.';
END $$;
