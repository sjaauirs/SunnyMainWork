-- ============================================================================
-- 🚀 Script    : SUN-702
-- 📌 Purpose   : Rollback - Update agreementColors for HAP
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-09-24
-- 🧾 Jira      : 702
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- ============================================================================

DO $$
DECLARE
  v_tenant_code text := '<HAP-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = tenant_attr
                      - '{ux,agreementColors,agreeButtonColor}'
                      - '{ux,agreementColors,agreeButtonLabelColor}'
                      - '{ux,agreementColors,declineButtonLabelColor}'
                      - '{ux,agreementColors,rewardsSplashButtonLabelColor}'
                      - '{ux,agreementColors,rewardsSplashButtonColor}'
                      - '{ux,agreementColors,rewardsSplashButtonBorderColor}'
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;
