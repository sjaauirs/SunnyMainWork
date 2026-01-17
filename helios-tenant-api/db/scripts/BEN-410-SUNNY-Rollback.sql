-- ============================================================================
-- 🚀 Script    : Rollback UX Button Styling from Tenant Attribute for SUNNY
-- 📌 Purpose   : Removes the "button" object inside "ux" from tenant_attr JSONB
-- 🧑 Author    : Ankush Gawande
-- 📅 Date      : 2025-10-09
-- 🧾 Jira      : BEN-273
-- ⚠️ Inputs    : <SUNNY-TENANT-CODE>
-- 📤 Output    : Removes the "button" object from the JSONB structure
-- 🔗 Script URL: <NA>
-- 📝 Notes     : This script assumes the tenant_attr column is of type JSONB.
--               If "button" does not exist, no changes will be made.
-- ============================================================================
DO $$
DECLARE
  v_tenant_code TEXT := '<SUNNY-TENANT-CODE>';  
BEGIN 
    -- Remove "button" object from "ux"    
	UPDATE tenant.tenant
	SET tenant_attr = jsonb_set(
	  tenant_attr,
	  '{ux}',
	  (tenant_attr->'ux')::jsonb - 'button',
	  true
	)
	WHERE tenant_code = v_tenant_code;

    -- Confirmation message
    RAISE NOTICE '[Rollback] Removed "button" object from tenant: %', v_tenant_code; 
END $$;