-- ============================================================================
-- 🚀 Script    : Add 'displayBancorpCopyright' flag for SUNNY tenant
-- 📌 Purpose   : Enables the Bancorp copyright display in tenant configuration
-- 🧑 Author    : Riaz Ahmed
-- 📅 Date      : 2025-11-04
-- 🧾 Jira      : SUN-917
-- ⚠️ Inputs    : v_tenant_codes (array of tenant codes)
-- 📤 Output    : Adds "displayBancorpCopyright": true under tenant_attr
-- 📝 Notes     : 
--   - Adds the displayBancorpCopyright flag if missing, or updates it to true.
--   - Existing tenant_attr keys remain intact.
--   - update_ts and update_user are refreshed.
--   - Safe to re-run this script
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'SUNNY-TENANT-CODE'
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
            'true'::jsonb,
            true
        ),
        update_ts = NOW(),
        update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        RAISE NOTICE '✅ Tenant % → % rows updated', v_tenant_code, v_rowcount;
    END LOOP;
END $$;
