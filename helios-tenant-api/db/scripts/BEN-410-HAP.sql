-- ============================================================================
-- 🚀 Script    : Add UX button styling to Tenant Attribute for HAP
-- 📌 Purpose   : Adds or replaces the "button" object inside "ux" in tenant_attribute JSONB
-- 🧑 Author    : Ankush Gawande
-- 📅 Date      : 2025-10-09
-- 🧾 Jira      : BEN-410
-- ⚠️ Inputs    : <HAP-TENANT-CODE>
-- 📤 Output    : Updated tenant_attribute JSONB with new button styling
-- 🔗 Script URL: <NA>
-- 📝 Notes     : This script assumes the tenant_attribute column is of type JSONB.
--               If "button" already exists, it will be overwritten.

-- ============================================================================

DO $$
DECLARE
  v_tenant_code TEXT := '<HAP-TENANT-CODE>';
BEGIN
  -- Add new key to tenant_attr JSONB  
	UPDATE tenant.tenant
	SET tenant_attr = jsonb_set(
	    tenant_attr,
	    '{ux, button}',
	    jsonb_build_object(
	        'borderWidth', 1,
	        'primaryTextColor', '#181D27',
	        'primaryBgColor', '#FFFFFF',
	        'borderColor', '#181D27'
	    ),
	    true
	)
	WHERE tenant_code = v_tenant_code;

  -- Confirmation message
  RAISE NOTICE '[Information] Tenant attribute updated successfully for tenant: %', v_tenant_code;
END $$;
