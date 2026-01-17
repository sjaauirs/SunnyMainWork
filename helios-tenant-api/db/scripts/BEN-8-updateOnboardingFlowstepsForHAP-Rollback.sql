-- ============================================================================
-- 📌 Purpose   : 
--   - Rollback onboarding_flow configuration for HAP-related cohorts
--   - Soft deletes the specified flow and its steps (delete_nbr = pk)
--   - Restores system state to pre-change condition
-- 🧑 Author    : Saurabh
-- 📅 Date      : 2025-09-25
-- 🧾 Jira      : BEN-8
-- ⚠️ Inputs    : 
--       v_tenant_code   (Tenant code, e.g., '<HAP-TENANT-CODE>')
--       v_cohort_code   (Resolved dynamically by cohort_name)
--       v_flow_name     ('onboarding_flow')
--       v_version_nbr   (Version of flow to rollback, default = 1)
-- 📤 Output    : 
--   - tenant.flow_step: Marks all active steps for this flow as deleted
--   - tenant.flow: Marks the flow itself as deleted
--   - Logs row counts for rollback actions
-- 🔗 Script URL: https://github.com/SunnyRewards/helios-tenant-api/blob/develop/db/scripts/BEN-8-updateOnboardingFlowstepsForHAP-Rollback.sql
-- 📝 Notes     : 
--   - Uses soft delete pattern (delete_nbr = pk) instead of hard deletes
--   - Idempotent: safe to re-run, no error if flow not found
--   - Must match delete_nbr convention in schema
-- ============================================================================

-- =====================================================
-- Rollback Script: Restore soft deleted flow + steps
-- =====================================================
DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- <<<<<< Input tenant code
    v_cohort_code TEXT := NULL;                -- <<<<<< Input cohort code (nullable allowed)
    v_flow_name   TEXT := 'onboarding_flow';
    v_version_nbr INT  := 1;

    v_flow_pk BIGINT;
    v_rowcount INT;
BEGIN
    RAISE NOTICE '================= FLOW ROLLBACK START =================';

    ----------------------------------------------------------------------
    -- Look up cohort_code dynamically
    ----------------------------------------------------------------------
    SELECT cohort_code
    INTO v_cohort_code
    FROM cohort.cohort
    WHERE cohort_name = 'healthy_living_rewards'
      AND delete_nbr = 0
    LIMIT 1;

    IF v_cohort_code IS NULL THEN
        RAISE EXCEPTION '❌ Cohort "healthy_living_rewards" not found in cohort.cohort';
    END IF;

    RAISE NOTICE 'Using cohort_code = %', v_cohort_code;

    -- Get flow primary key
    SELECT pk INTO v_flow_pk
    FROM tenant.flow
    WHERE tenant_code = v_tenant_code
      AND (cohort_code = v_cohort_code OR (v_cohort_code IS NULL AND cohort_code IS NULL))
      AND flow_name = v_flow_name
      AND version_nbr = v_version_nbr;

    IF v_flow_pk IS NULL THEN
        RAISE NOTICE '⚠️ No flow found to rollback for tenant=% flow=% v=%', v_tenant_code, v_flow_name, v_version_nbr;
        RETURN;
    END IF;

    -- Rollback steps: mark deleted rows with delete_nbr = pk
    UPDATE tenant.flow_step
    SET delete_nbr = pk,
        update_ts = NOW(),
        update_user = 'SYSTEM'
    WHERE flow_fk = v_flow_pk
      AND delete_nbr = 0;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;
    IF v_rowcount > 0 THEN
        RAISE NOTICE '♻️ Rolled back % flow_step(s) for flow_pk = %', v_rowcount, v_flow_pk;
    END IF;

    -- Rollback flow: mark deleted rows with delete_nbr = pk
    UPDATE tenant.flow
    SET delete_nbr = pk,
        update_ts = NOW(),
        update_user = 'SYSTEM'
    WHERE pk = v_flow_pk
      AND delete_nbr = 0;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;
    IF v_rowcount > 0 THEN
        RAISE NOTICE '♻️ Rolled back flow row pk = %', v_flow_pk;
    END IF;

    RAISE NOTICE '================= FLOW ROLLBACK COMPLETE =================';
END $$;

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- <<<<<< Input tenant code
    v_cohort_code TEXT := NULL;                -- <<<<<< Input cohort code (nullable allowed)
    v_flow_name   TEXT := 'onboarding_flow';
    v_version_nbr INT  := 1;

    v_flow_pk BIGINT;
    v_rowcount INT;
BEGIN
    RAISE NOTICE '================= FLOW ROLLBACK START =================';

    ----------------------------------------------------------------------
    -- Look up cohort_code dynamically
    ----------------------------------------------------------------------
    SELECT cohort_code
    INTO v_cohort_code
    FROM cohort.cohort
    WHERE cohort_name = 'hap_flex_rewards'
      AND delete_nbr = 0
    LIMIT 1;

    IF v_cohort_code IS NULL THEN
        RAISE EXCEPTION '❌ Cohort "hap_flex_rewards" not found in cohort.cohort';
    END IF;

    RAISE NOTICE 'Using cohort_code = %', v_cohort_code;

    -- Get flow primary key
    SELECT pk INTO v_flow_pk
    FROM tenant.flow
    WHERE tenant_code = v_tenant_code
      AND (cohort_code = v_cohort_code OR (v_cohort_code IS NULL AND cohort_code IS NULL))
      AND flow_name = v_flow_name
      AND version_nbr = v_version_nbr;

    IF v_flow_pk IS NULL THEN
        RAISE NOTICE '⚠️ No flow found to rollback for tenant=% flow=% v=%', v_tenant_code, v_flow_name, v_version_nbr;
        RETURN;
    END IF;

    -- Rollback steps: mark deleted rows with delete_nbr = pk
    UPDATE tenant.flow_step
    SET delete_nbr = pk,
        update_ts = NOW(),
        update_user = 'SYSTEM'
    WHERE flow_fk = v_flow_pk
      AND delete_nbr = 0;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;
    IF v_rowcount > 0 THEN
        RAISE NOTICE '♻️ Rolled back % flow_step(s) for flow_pk = %', v_rowcount, v_flow_pk;
    END IF;

    -- Rollback flow: mark deleted rows with delete_nbr = pk
    UPDATE tenant.flow
    SET delete_nbr = pk,
        update_ts = NOW(),
        update_user = 'SYSTEM'
    WHERE pk = v_flow_pk
      AND delete_nbr = 0;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;
    IF v_rowcount > 0 THEN
        RAISE NOTICE '♻️ Rolled back flow row pk = %', v_flow_pk;
    END IF;

    RAISE NOTICE '================= FLOW ROLLBACK COMPLETE =================';
END $$;

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- <<<<<< Input tenant code
    v_cohort_code TEXT := NULL;                -- <<<<<< Input cohort code (nullable allowed)
    v_flow_name   TEXT := 'onboarding_flow';
    v_version_nbr INT  := 1;

    v_flow_pk BIGINT;
    v_rowcount INT;
BEGIN
    RAISE NOTICE '================= FLOW ROLLBACK START =================';

    ----------------------------------------------------------------------
    -- Look up cohort_code dynamically
    ----------------------------------------------------------------------
    SELECT cohort_code
    INTO v_cohort_code
    FROM cohort.cohort
    WHERE cohort_name = 'henry_ford_health_flex_rewards'
      AND delete_nbr = 0
    LIMIT 1;

    IF v_cohort_code IS NULL THEN
        RAISE EXCEPTION '❌ Cohort "henry_ford_health_flex_rewards" not found in cohort.cohort';
    END IF;

    RAISE NOTICE 'Using cohort_code = %', v_cohort_code;

    -- Get flow primary key
    SELECT pk INTO v_flow_pk
    FROM tenant.flow
    WHERE tenant_code = v_tenant_code
      AND (cohort_code = v_cohort_code OR (v_cohort_code IS NULL AND cohort_code IS NULL))
      AND flow_name = v_flow_name
      AND version_nbr = v_version_nbr;

    IF v_flow_pk IS NULL THEN
        RAISE NOTICE '⚠️ No flow found to rollback for tenant=% flow=% v=%', v_tenant_code, v_flow_name, v_version_nbr;
        RETURN;
    END IF;

    -- Rollback steps: mark deleted rows with delete_nbr = pk
    UPDATE tenant.flow_step
    SET delete_nbr = pk,
        update_ts = NOW(),
        update_user = 'SYSTEM'
    WHERE flow_fk = v_flow_pk
      AND delete_nbr = 0;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;
    IF v_rowcount > 0 THEN
        RAISE NOTICE '♻️ Rolled back % flow_step(s) for flow_pk = %', v_rowcount, v_flow_pk;
    END IF;

    -- Rollback flow: mark deleted rows with delete_nbr = pk
    UPDATE tenant.flow
    SET delete_nbr = pk,
        update_ts = NOW(),
        update_user = 'SYSTEM'
    WHERE pk = v_flow_pk
      AND delete_nbr = 0;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;
    IF v_rowcount > 0 THEN
        RAISE NOTICE '♻️ Rolled back flow row pk = %', v_flow_pk;
    END IF;

    RAISE NOTICE '================= FLOW ROLLBACK COMPLETE =================';
END $$;