-- ============================================================================
-- 🚀 Script    : Script to Rollback flag "displayAlternateDialer" in tenant_attr
-- 📌 Purpose   : Rollback the flag in tenant
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 24-09-2025
-- 🧾 Jira      : RES-150 & RES-560(Sub-task)
-- ⚠️ Inputs    : No Input required
-- 📤 Output    : It will Rollback the flag "displayAlternateDialer" for all tenants in tenant.tenant table
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ============================================================================

DO $$
DECLARE
    v_updated_count INT := 0;
BEGIN
    RAISE NOTICE '[Information] Starting rollback: Removing "displayAlternateDialer" flag from tenant.tenant.tenant_attr';

    UPDATE tenant.tenant t
    SET tenant_attr = tenant_attr - 'displayAlternateDialer'
    WHERE t.delete_nbr = 0
      AND tenant_attr ? 'displayAlternateDialer';

    GET DIAGNOSTICS v_updated_count = ROW_COUNT;

    IF v_updated_count > 0 THEN
        RAISE NOTICE '[Information] Successfully removed flag from % row(s)', v_updated_count;
    ELSE
        RAISE NOTICE '[Information] No rows required rollback — flag does not exist in any row';
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '[Error] Unexpected error occurred while removing flag: %', SQLERRM;
        RAISE;
END $$;
