-- ============================================================================
-- 🚀 Script    : Rollback Terms and Conditions Visibility Flag from Tenant Attribute
-- 📌 Purpose   : Removes the top-level JSONB key `isTermsAndConditionVisibleForOrderCard` 
--               from the `tenant_attr` column in `tenant.tenant` table.
-- 🧑 Author    : Ankush Gawande
-- 📅 Date      : 2025-09-25
-- 🧾 Jira      : BEN-273
-- ⚠️ Inputs    : <NAVITUS-TENANT-CODE>
-- 📤 Output    : Removes the specified key from the JSONB structure
-- 🔗 Script URL: <NA>
-- 📝 Notes     : This script assumes the `tenant_attr` column is of type JSONB.
--               If the key does not exist, no changes will be made.
-- ============================================================================

DO $$
DECLARE
  v_tenant_code TEXT := '<NAVITUS-TENANT-CODE>';
BEGIN
  -- 🔄 Remove key from tenant_attr JSONB
  UPDATE tenant.tenant
  SET tenant_attr = tenant_attr - 'isTermsAndConditionVisibleForOrderCard'
  WHERE tenant_code = v_tenant_code;

  -- 📢 Confirmation message
  RAISE NOTICE '[Rollback] Removed key from tenant attribute for tenant: %', v_tenant_code;
END $$;
