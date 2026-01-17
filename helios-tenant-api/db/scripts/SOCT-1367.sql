-- ============================================================================
-- Script Name : update_kp_ux_colors.sql
-- Author      : Vinod Kumar Ullaganti
-- Created On  : 2025-07-18
-- JIRA ID     : SOCT-1367
-- Description : 
--   Updates UX-related color settings in the `tenant_attr` JSONB column
--   for a specific Kaiser Permanente tenant in the `tenant.tenant` table.
-- 
--   Specifically updates:
--     1. ux.agreementColors:
--        - agreeButtonColor            => #0078B3
--        - agreeButtonLabelColor       => #FFFFFF
--        - declineButtonLabelColor     => #0078B3
--
--     2. ux.taskTileColors.inProgressTextFgColor => #0078B3
--
-- Notes:
--   - Replace <KP_TENANT_CODE> with the actual tenant code before executing.
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<KP_TENANT_CODE>';  -- 🔁 Replace with actual KP tenant code
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
                          jsonb_set(
                              tenant_attr,
                              '{ux,agreementColors}',
                              '{
                                "agreeButtonColor": "#0078B3",
                                "agreeButtonLabelColor": "#FFFFFF",
                                "declineButtonLabelColor": "#0078B3"
                              }'::jsonb,
                              true
                          ),
                          '{ux,taskTileColors,inProgressTextFgColor}',
                          '"#0078B3"',
                          true
                      )
    WHERE tenant_code = v_tenant_code AND delete_nbr = 0;

    RAISE NOTICE 'Updated agreement and taskTileColors for tenant: %', v_tenant_code;
END $$;

-- ============================================================================
-- Script Name : Update UX Config Colors for Tenant
-- Author      : Vinod Kumar Ullaganti
-- Created On  : 2025-07-22
-- JIRA ID     : SOCT-1367, SUN-486
-- Description : 
--   - Updates UX color settings inside the `tenant.tenant` table's tenant_attr JSONB.
--   - Applies updates only for the given tenant_code and delete_nbr = 0.
--   - Affects the following nested UX color settings:
--       1. ux.onboardingColors.stepActiveColor       → "#E27025"
--       2. ux.agreementColors                        → Set of agree/decline button colors
--       3. ux.taskTileColors.inProgressTextFgColor   → "#326F91"
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<NAVIUS_TENANT_CODE>';  -- 🔁 Replace with actual NAVIUS tenant code
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
        jsonb_set(
            jsonb_set(
                tenant_attr,
                '{ux,onboardingColors,stepActiveColor}',
                '"#E27025"',
                true
            ),
            '{ux,agreementColors}',
            '{
              "agreeButtonColor": "#0B3B60",
              "agreeButtonLabelColor": "#FFFFFF",
              "declineButtonLabelColor": "#0B3B60"
            }'::jsonb,
            true
        ),
        '{ux,taskTileColors,inProgressTextFgColor}',
        '"#326F91"',
        true
    )
    WHERE tenant_code = v_tenant_code AND delete_nbr = 0;

    RAISE NOTICE 'UX config updated successfully for tenant: %', v_tenant_code;
END $$;  