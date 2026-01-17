-- ============================================================================
-- 🚀 Script    : Rollback - Update rewardsSplashButtonColor and rewardsSplashButtonLabelColor
-- 📌 Purpose   : Rollback - Update rewardsSplashButtonColor and rewardsSplashButtonLabelColor
-- 🧑 Author    : Preeti
-- 📅 Date      : 09/24/2025
-- 🧾 Jira      : BEN-631
-- ⚠️ Inputs    : <HAP-TENANT-CODE>
-- ============================================================================
DO $$
DECLARE
  hap_tenant_code text := '<HAP-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      jsonb_set(
                        tenant_attr,
                        '{ux,agreementColors,rewardsSplashButtonColor}',
                        '"#181D27"'::jsonb,
                        true
                      ),
                      '{ux,agreementColors,rewardsSplashButtonLabelColor}',
                      '"#181D27"'::jsonb,
                      true
                    )
  WHERE tenant_code = hap_tenant_code
    AND delete_nbr = 0;
END $$;
