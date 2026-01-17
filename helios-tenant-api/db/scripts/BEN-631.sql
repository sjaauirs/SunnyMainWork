-- ============================================================================
-- 🚀 Script    : Update rewardsSplashButtonColor and rewardsSplashButtonLabelColor
-- 📌 Purpose   : Update rewardsSplashButtonColor and rewardsSplashButtonLabelColor
-- 🧑 Author    : Preeti
-- 📅 Date      : 09/24/2025
-- 🧾 Jira      : BEN-631
-- ⚠️ Inputs    : <HAP-TENANT-CODE>
-- ============================================================================

DO $$
DECLARE
  v_tenant_code text := '<HAP-TENANT-CODE>';
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
                      '"#FFFFFF"'::jsonb,
                      true
                    )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;
