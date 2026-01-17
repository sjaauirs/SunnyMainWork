-- ============================================================================
-- 🚀 Script    : Update shopColors and walletColors
-- 📌 Purpose   : Updates shopColors and walletColors
--               for the given tenant in tenant.tenant.tenant_attr.
-- 🧑 Author    : Charan
-- 📅 Date      : 10/08/2025
-- 🧾 Jira      : BEN-1409
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : tenant_attr updated with new color value
-- 📝 Notes     : Creates/updates the path if missing.
-- ============================================================================ 

DO $$
DECLARE
  v_tenant_code text := '<HAP-TENANT-CODE>';
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
        '{ux,shopColors,disabledTextColor}', '"#909399"'::jsonb, true),
        '{ux,shopColors,errorColor}', '"#8A210B"'::jsonb, true),
        '{ux,shopColors,disabledBorderColor}', '"#C9CACC"'::jsonb, true),
        '{ux,shopColors,disabledButtonBorderColor}', '"#ABAEB2"'::jsonb, true),
        '{ux,shopColors,disabledButtonBackgroundColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,shopColors,zipCodeButtonBgColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,shopColors,zipCodeButtonLabelColor}', '"#181D27"'::jsonb, true),
        '{ux,shopColors,zipCodeButtonBorderColor}', '"#181D27"'::jsonb, true),
        '{ux,shopColors,scanItemButtonBgColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,shopColors,scanItemButtonBorderColor}', '"#181D27"'::jsonb, true),
        '{ux,shopColors,scanItemButtonLabelColor}', '"#181D27"'::jsonb, true),
        '{ux,shopColors,getDirectionsButtonBgColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,shopColors,getDirectionsButtonBorderColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,shopColors,getDirectionsButtonLabelColor}', '"#181D27"'::jsonb, true),
        '{ux,walletColors,seeMorePrimaryButtonBgColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,walletColors,seeMorePrimaryButtonBorderColor}', '"#181D27"'::jsonb, true),
        '{ux,walletColors,seeMorePrimaryButtonLabelColor}', '"#181D27"'::jsonb, true),
        '{ux,walletColors,seeMoreSecondaryButtonBgColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,walletColors,seeMoreSecondaryButtonBorderColor}', '"#FFFFFF"'::jsonb, true),
        '{ux,walletColors,seeMoreSecondaryButtonLabelColor}', '"#181D27"'::jsonb, true)
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
  RAISE NOTICE 'Updated ux.shopColors and ux.walletColors for tenant %', v_tenant_code;
END $$;