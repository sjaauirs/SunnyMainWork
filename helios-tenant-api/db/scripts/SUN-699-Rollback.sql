-- ============================================================================
-- 📌 Script       : HAP_OnboardingFlow_Rollback.sql
-- 🎯 Purpose      : Rollback Fix SUN-699
--                  Revert onboarding flow changes for HAP tenant so that
--                  `agreement_screen` step’s on_failure path points back
--                  to itself (original configuration before fix).
-- 🏷️ Jira         : SUN-699 / BEN-6
-- 🧑 Author       : <Deepthi Muttineni>
-- 📅 Date         : <2025-10-02>
-- ⚠️ Notes        :
--    - Use only if the SUN-699 fix (pointing on_failure to splash) needs to be reverted.
--    - Restores `agreement_screen.on_failure` → `agreement_screen`.
--    - Applies only to HAP tenant.
DO $$
DECLARE
    v_tenant_code TEXT := '<HAP Tenant Code>';  -- HAP tenant code
    v_agreement BIGINT;
BEGIN
    -- Get agreement_screen component id
    SELECT pk INTO v_agreement
    FROM tenant.component_catalogue
    WHERE component_name = 'agreement_screen'
      AND delete_nbr = 0;

    IF v_agreement IS NULL THEN
        RAISE EXCEPTION 'agreement_screen not found in component_catalogue';
    END IF;

    -- Rollback: set on_failure back to agreement_screen (itself)
    UPDATE tenant.flow_step
    SET on_failure_component_catalogue_fk = v_agreement,
        update_ts = NOW(),
        update_user = 'ROLLBACK_SCRIPT'
    WHERE flow_fk IN (
        SELECT pk FROM tenant.flow
        WHERE tenant_code = v_tenant_code
          AND flow_name = 'onboarding_flow'
          AND delete_nbr = 0
    )
    AND current_component_catalogue_fk = v_agreement;

    RAISE NOTICE '♻️ Rollback complete: agreement_screen.on_failure reset to itself for tenant %', v_tenant_code;
END $$;
