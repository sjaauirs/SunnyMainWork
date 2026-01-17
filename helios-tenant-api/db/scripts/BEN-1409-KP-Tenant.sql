-- ============================================================================
-- 🚀 Script    : Update shopColors and walletColors
-- 📌 Purpose   : Updates shopColors and walletColors
--               for the given tenant in tenant.tenant.tenant_attr.
-- 🧑 Author    : Charan
-- 📅 Date      : 10/08/2025
-- 🧾 Jira      : BEN-1409
-- ⚠️ Inputs    : KP-TENANT-CODE
-- 📤 Output    : tenant_attr updated with new color value
-- 📝 Notes     : Creates/updates the path if missing.
-- ============================================================================ 

DO $$
DECLARE
  v_tenant_code text := '<KP-TENANT-CODE>'; -- replace with actual tenant code
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr =
      jsonb_set( 
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
      jsonb_set(
        tenant_attr,
        '{ux,shopColors,disabledTextColor}', '"#858D9C"'::jsonb, true),
        '{ux,shopColors,errorColor}', '"#8A210B"'::jsonb, true),
        '{ux,shopColors,disabledBorderColor}', '"#B3B7C1"'::jsonb, true),
        '{ux,shopColors,disabledButtonBorderColor}', '"#D3D6DC"'::jsonb, true),
        '{ux,shopColors,disabledButtonBackgroundColor}', '"#D3D6DC"'::jsonb, true),
        '{ux,shopColors,zipCodeButtonBgColor}', '"#0078B3"'::jsonb, true),
        '{ux,shopColors,zipCodeButtonLabelColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,shopColors,zipCodeButtonBorderColor}', '"#0078B3"'::jsonb, true),
        '{ux,shopColors,scanItemButtonBgColor}', '"#0078B3"'::jsonb, true),
        '{ux,shopColors,scanItemButtonBorderColor}', '"#0078B3"'::jsonb, true),
        '{ux,shopColors,scanItemButtonLabelColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,shopColors,getDirectionsButtonBgColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,shopColors,getDirectionsButtonBorderColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,shopColors,getDirectionsButtonLabelColor}', '"#0078B3"'::jsonb, true),
        '{ux,walletColors,seeMorePrimaryButtonBgColor}', '"#0078B3"'::jsonb, true),
        '{ux,walletColors,seeMorePrimaryButtonBorderColor}', '"#0078B3"'::jsonb, true),
        '{ux,walletColors,seeMorePrimaryButtonLabelColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,walletColors,seeMoreSecondaryButtonBgColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,walletColors,seeMoreSecondaryButtonBorderColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,walletColors,seeMoreSecondaryButtonLabelColor}', '"#0078B3"'::jsonb, true)
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'Added ux.shopColors and ux.walletColors for tenant %', v_tenant_code;
END $$;
