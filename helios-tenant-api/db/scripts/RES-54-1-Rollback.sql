-- ============================================================================
-- 🚀 Script    : Add enableActivityTracking Flag to Tenant Attribute
-- 📌 Purpose   : Adds a new top-level JSONB key `enableActivityTracking` 
--               with value `false` to the `tenant_attr` column in `tenant.tenant` table to make terms
--				 condition checkbox show/hide
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-09-29
-- 🧾 Jira      : RES-54
-- ⚠️ Inputs    : <KP-TENANT-CODE>
-- 📤 Output    : Updates the JSONB structure with the new key-value pair
-- 🔗 Script URL: <NA>
-- 📝 Notes     : This script assumes the `tenant_attr` column is of type JSONB.
--               If the key already exists, it will be overwritten.
-- ============================================================================

DO $$
DECLARE
  v_tenant_code TEXT := '<KP-TENANT-CODE>';
BEGIN
  -- Add new key to tenant_attr JSONB
  UPDATE tenant.tenant
  SET tenant_attr = tenant_attr || jsonb_build_object('enableActivityTracking', false)
  WHERE tenant_code = v_tenant_code;

  -- Confirmation message
  RAISE NOTICE '[Information] Tenant attribute updated successfully for tenant: %', v_tenant_code;
END $$;