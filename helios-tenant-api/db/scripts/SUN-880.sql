-- ============================================================================
-- 🚀 Script    : Update mycardColors.walletBgColor
-- 📌 Purpose   : Updates the mycardColors.walletBgColor to "#F7F4F0"
--               for the given tenant in tenant.tenant.tenant_attr.
-- 🧑 Author    : Pranav
-- 📅 Date      : 10/31/2025
-- 🧾 Jira      : SUN-880
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : tenant_attr updated with new color value
-- 📝 Notes     : Creates/updates the path if missing.
-- ============================================================================

DO $$
DECLARE
  v_tenant_code text := '<HAP-TENANT-CODE>'; -- replace with actual tenant code
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      tenant_attr,
                      '{ux,mycardColors,walletBgColor}',
                      '"#F7F4F0"'::jsonb,
                      true
                    )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'Added ux.mycardColors.walletBgColor = #F7F4F0 for tenant %', v_tenant_code;
END $$;
