-- ============================================================================
-- 🔄 Script    : Rollback 'displayBancorpCopyright' flag for NAVITUS tenant
-- 📌 Purpose   : Sets 'displayBancorpCopyright' flag to false in tenant_attr
-- 🧑 Author    : Riaz Ahmed
-- 📅 Date      : 2025-11-04
-- 🧾 Jira      : SUN-917
-- ⚠️ Inputs    : v_tenant_codes (array of tenant codes)
-- 📤 Output    : Updates "displayBancorpCopyright": false under tenant_attr
-- 📝 Notes     : 
--   - Reverts the flag value to false (instead of removing it).
--   - Existing tenant_attr keys remain intact.
--   - update_ts and update_user are refreshed.
--   - Safe to re-run this script.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_rowcount INT := 0;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
                              tenant_attr,
                              '{displayBancorpCopyright}',
                              'false'::jsonb,
                              true
                          ),
            update_ts   = NOW(),
            update_user = 'SYSTEM-ROLLBACK'
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        RAISE NOTICE '♻️ Tenant % → % rows rolled back (displayBancorpCopyright=false)', v_tenant_code, v_rowcount;
    END LOOP;
END $$;
