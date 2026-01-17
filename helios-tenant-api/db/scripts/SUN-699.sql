--============================================================================
-- 📌 Script       : HAP_OnboardingFlow_Fix.sql
-- 🎯 Purpose      : Fix SUN-699
--                  Ensure that when a member declines Wellness Agreement,
--                  they are redirected to splash (not advanced in onboarding).
-- 🏷️ Jira         : SUN-699 / BEN-6
-- 🧑 Author       : <Deepthi Muttineni>
-- 📅 Date         : <2025-10-02>
-- ⚠️ Notes        : 
--    - This script updates flow_step so that the `agreement_screen` step’s 
--      on_failure path points to `rewards_splash_screen` instead of itself.
--    - Applies only to HAP tenant.
DO $$
DECLARE
    v_tenant_code TEXT := '<HAP Tenant Code>';  -- HAP tenant code
    v_splash BIGINT;
BEGIN
    -- Get splash component id
    SELECT pk INTO v_splash
    FROM tenant.component_catalogue
    WHERE component_name = 'rewards_splash_screen'
      AND delete_nbr = 0;

    IF v_splash IS NULL THEN
        RAISE EXCEPTION 'rewards_splash_screen not found in component_catalogue';
    END IF;

    -- Update on_failure for agreement_screen across all flow versions
    UPDATE tenant.flow_step
    SET on_failure_component_catalogue_fk = v_splash,
        update_ts = NOW(),
        update_user = 'SYSTEM'
    WHERE flow_fk IN (
        SELECT pk FROM tenant.flow
        WHERE tenant_code = v_tenant_code
          AND flow_name = 'onboarding_flow'
          AND delete_nbr = 0
    )
    AND current_component_catalogue_fk = (
        SELECT pk FROM tenant.component_catalogue
        WHERE component_name = 'agreement_screen'
          AND delete_nbr = 0
    );

    RAISE NOTICE ' Updated: agreement_screen on_failure now points to splash for tenant %', v_tenant_code;
END $$;