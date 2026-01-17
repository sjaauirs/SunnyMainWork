-- ============================================================================
-- 🚀 Script    : Rollback commonColors.hyperLinkTextColor
-- 📌 Purpose   : Removes the commonColors.hyperLinkTextColor key from
--               tenant.tenant.tenant_attr for the given tenant.
-- 🧑 Author    : Charan
-- 📅 Date      : 10/07/2025
-- 🧾 Jira      : BEN-458
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : tenant_attr rolled back (hyperLinkTextColor key removed)
-- 📝 Notes     : Safe to run multiple times; does nothing if key is absent.
-- ============================================================================

DO $$
DECLARE
  v_tenant_code text := '<HAP-TENANT-CODE>'; -- replace with actual tenant code
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = tenant_attr #- '{ux,commonColors,hyperLinkTextColor}'
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'Removed ux.commonColors.hyperLinkTextColor for tenant %', v_tenant_code;
END $$;
