-- ============================================================================
-- 🚀 Script    : Rollback Script - Remove "enableSelfAttestationCompletion" key from tenant_attr
-- 📌 Purpose   : Rollback of RES-1027 update that added the flag in tenant_attr
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 03-11-2025
-- 🧾 Jira      : RES-1027(subtask)
-- ⚠️ Inputs    : None (applies to all tenants with delete_nbr = 0)
-- 📤 Output    : Removes the "enableSelfAttestationCompletion" key safely from tenant_attr JSON
-- 🔗 Script URL: NA
-- 📝 Notes     : Does not modify any other keys in tenant_attr
-- ============================================================================

DO $$
DECLARE
    v_tenant RECORD;
    v_removed_count INT := 0;
    v_processed_count INT := 0;
    v_rowcount INT := 0;
BEGIN
    RAISE NOTICE '[Information] Starting rollback: removing "enableSelfAttestationCompletion" key from all active tenants...';

    FOR v_tenant IN
        SELECT tenant_code
        FROM tenant.tenant
        WHERE delete_nbr = 0
          AND tenant_attr ? 'enableSelfAttestationCompletion'
    LOOP
        v_processed_count := v_processed_count + 1;

        UPDATE tenant.tenant
        SET tenant_attr = tenant_attr - 'enableSelfAttestationCompletion'
        WHERE tenant_code = v_tenant.tenant_code
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        v_removed_count := v_removed_count + v_rowcount;

        RAISE NOTICE '[Rollback] Tenant % - Removed "enableSelfAttestationCompletion" key.', v_tenant.tenant_code;
    END LOOP;

    RAISE NOTICE '[Summary] Processed tenants : %', v_processed_count;
    RAISE NOTICE '[Summary] Keys removed      : %', v_removed_count;
    RAISE NOTICE '[Information] Rollback completed successfully.';

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '[Error] Unexpected error during rollback: %', SQLERRM;
        RAISE;
END $$;
