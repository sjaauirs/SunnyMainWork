-- ============================================================================
-- 🚀 Script    : Update shopColors.storeCardOpenLabelColor
-- 📌 Purpose   : Updates the shopColors.storeCardOpenLabelColor to "#0A855C"
--               for the given tenant in tenant.tenant.tenant_attr.
-- 🧑 Author    : Preeti
-- 📅 Date      : 09/29/2025
-- 🧾 Jira      : BEN-1000
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
                      '{ux,shopColors,storeCardOpenLabelColor}',
                      '"#0A855C"'::jsonb,
                      true
                    )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'Updated shopColors.storeCardOpenLabelColor to #0A855C for tenant %', v_tenant_code;
END $$;