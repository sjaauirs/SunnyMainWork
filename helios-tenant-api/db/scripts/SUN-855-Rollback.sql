-- ============================================================================
-- 🔁 Rollback : remove 'showAllTransactionFilter' from tenant_attr
-- 🧑 Author   : Riaz
-- 📅 Date     : 2025-10-31
-- 🧾 Jira     : SUN-855
-- ⚠️ Inputs   : HAP-TENANT-CODE
-- 📤 Output   : Removes JSON key; logs status messages via RAISE NOTICE
-- 📝 Notes    : Reverses the addition of 'showAllTransactionFilter'
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';
    v_affected    INTEGER := 0;
BEGIN
    UPDATE tenant.tenant
       SET tenant_attr = tenant_attr - 'showAllTransactionFilter'
     WHERE tenant_code = v_tenant_code
       AND delete_nbr = 0
       AND tenant_attr ? 'showAllTransactionFilter';

    GET DIAGNOSTICS v_affected = ROW_COUNT;

    IF v_affected > 0 THEN
        RAISE NOTICE 'Removed showAllTransactionFilter from tenant_attr for tenant: %', v_tenant_code;
    ELSE
        RAISE NOTICE 'No change: flag not present or tenant not found (tenant: %)', v_tenant_code;
    END IF;
END $$;