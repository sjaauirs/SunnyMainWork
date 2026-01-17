-- ============================================================================
-- 🚀 Script    : Rollback mycardColors.walletBgColor
-- 📌 Purpose   : Removes the mycardColors.walletBgColor key from
--               tenant.tenant.tenant_attr for the given tenant.
-- 🧑 Author    : Pranav
-- 📅 Date      : 10/31/2025
-- 🧾 Jira      : SUN-880
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : tenant_attr rolled back (walletBgColor key removed)
-- 📝 Notes     : Safe to run multiple times; does nothing if key is absent.
-- ============================================================================

DO $$
DECLARE
  v_tenant_code text := '<HAP-TENANT-CODE>'; -- replace with actual tenant code
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = tenant_attr #- '{ux,mycardColors,walletBgColor}'
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'Removed ux.mycardColors.walletBgColor for tenant %', v_tenant_code;
END $$;