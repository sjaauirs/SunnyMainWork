-- ============================================================================
-- 🚀 Script    : SUN-702
-- 📌 Purpose   : Update agreementColors for HAP
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
  SET tenant_attr = jsonb_set(
                      jsonb_set(
                        jsonb_set(
                          jsonb_set(
                            jsonb_set(
                              jsonb_set(
                                tenant_attr,
                                '{ux,agreementColors,agreeButtonColor}',
                                '"#181D27"'::jsonb,
                                true
                              ),
                              '{ux,agreementColors,agreeButtonLabelColor}',
                              '"#FFFFFF"'::jsonb,
                              true
                            ),
                            '{ux,agreementColors,declineButtonLabelColor}',
                            '"#181D27"'::jsonb,
                            true
                          ),
                          '{ux,agreementColors,rewardsSplashButtonLabelColor}',
                          '"#181D27"'::jsonb,
                          true
                        ),
                        '{ux,agreementColors,rewardsSplashButtonColor}',
                        '"#FFFFFF"'::jsonb,
                        true
                      ),
                      '{ux,agreementColors,rewardsSplashButtonBorderColor}',
                      '"#181D27"'::jsonb,
                      true
                    )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;