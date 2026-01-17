-- ============================================================================
-- 🚀 Script    : add 'showAllTransactionFilter' to tenant_attr
-- 📌 Purpose   : Add 'showAllTransactionFilter' to tenant_attr for HAP tenant
-- 🧑 Author    : Riaz
-- 📅 Date      : 2025-10-31
-- 🧾 Jira      : SUN-855
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : Updates JSON structure; logs status messages via RAISE NOTICE
-- 🔗 Script URL: NA
-- 📝 Notes     : 
--   - Adds 'showAllTransactionFilter' to tenant_attr.
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(tenant_attr, '{showAllTransactionFilter}', 'true', true)
    WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

    RAISE NOTICE 'Added showAllTransactionFilter to tenant_attr for tenant: %', v_tenant_code;
END $$;