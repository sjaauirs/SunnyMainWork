-- ============================================================================
-- 🚀 Script    : Rollback forYouColors in tenant_attr
-- 📌 Purpose   : Removes the forYouColors theme configuration from tenant_attr
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-09-30
-- 🧾 Jira      : BEN-252
-- ⚠️ Inputs    : 
--    - v_tenant_code (Tenant Code, e.g., <TENANT-CODE>)
-- 📤 Output    : forYouColors JSON object removed from tenant_attr
-- ============================================================================

DO $$
DECLARE
  v_tenant_code TEXT := '<TENANT-CODE>'; -- Replace with tenant code
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = tenant_attr #- '{ux,forYouColors}'
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'forYouColors removed for tenant %', v_tenant_code;
END $$;
