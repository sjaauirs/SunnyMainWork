-- ============================================================================
-- 🚀 Script    : Update commonColors.hyperLinkTextColor
-- 📌 Purpose   : Updates the commonColors.hyperLinkTextColor to "#005572"
--               for the given tenant in tenant.tenant.tenant_attr.
-- 🧑 Author    : Charan
-- 📅 Date      : 10/07/2025
-- 🧾 Jira      : BEN-458
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
                      '{ux,commonColors,hyperLinkTextColor}',
                      '"#005572"'::jsonb,
                      true
                    )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'Added ux.commonColors.hyperLinkTextColor = #005572 for tenant %', v_tenant_code;
END $$;
