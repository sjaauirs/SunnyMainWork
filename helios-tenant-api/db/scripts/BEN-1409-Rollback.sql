-- ============================================================================
-- 🚀 Script    : Rollback - Update shopColors and walletColors
-- 📌 Purpose   : Rollback - Updates shopColors and walletColors
--               for the given tenant in tenant.tenant.tenant_attr.
-- 🧑 Author    : Charan
-- 📅 Date      : 10/08/2025
-- 🧾 Jira      : BEN-1409
-- ⚠️ Inputs    : TENANT-CODE
-- 📤 Output    : tenant_attr updated with new color value
-- 📝 Notes     : Creates/updates the path if missing.
-- ============================================================================ 

DO $$
DECLARE
  v_tenant_code text := '<TENANT-CODE>'; -- replace with actual tenant code
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr =
      tenant_attr
      -- shopColors
      #- '{ux,shopColors,disabledTextColor}'
      #- '{ux,shopColors,errorColor}'
      #- '{ux,shopColors,disabledBorderColor}'
      #- '{ux,shopColors,disabledButtonBorderColor}'
      #- '{ux,shopColors,disabledButtonBackgroundColor}'
      #- '{ux,shopColors,zipCodeButtonBgColor}'
      #- '{ux,shopColors,zipCodeButtonLabelColor}'
      #- '{ux,shopColors,zipCodeButtonBorderColor}'
      #- '{ux,shopColors,scanItemButtonBgColor}'
      #- '{ux,shopColors,scanItemButtonBorderColor}'
      #- '{ux,shopColors,scanItemButtonLabelColor}'
      #- '{ux,shopColors,getDirectionsButtonBgColor}'
      #- '{ux,shopColors,getDirectionsButtonBorderColor}'
      #- '{ux,shopColors,getDirectionsButtonLabelColor}'
      -- walletColors
      #- '{ux,walletColors,seeMorePrimaryButtonBgColor}'
      #- '{ux,walletColors,seeMorePrimaryButtonBorderColor}'
      #- '{ux,walletColors,seeMorePrimaryButtonLabelColor}'
      #- '{ux,walletColors,seeMoreSecondaryButtonBgColor}'
      #- '{ux,walletColors,seeMoreSecondaryButtonBorderColor}'
      #- '{ux,walletColors,seeMoreSecondaryButtonLabelColor}'
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'Removed ux.shopColors/* and ux.walletColors/* for tenant %', v_tenant_code;
END $$;
