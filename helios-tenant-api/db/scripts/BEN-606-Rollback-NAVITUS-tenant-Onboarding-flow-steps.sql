-- =====================================================
-- Rollback Script: Restore soft deleted flow + steps
-- =====================================================
DO $$
DECLARE
    v_tenant_code TEXT := '<NAVITUS-TENANT-CODE>';  -- <<<<<< Input tenant code
    v_cohort_code TEXT := NULL;                -- <<<<<< Input cohort code (nullable allowed)
    v_flow_name   TEXT := 'onboarding_flow';
    v_version_nbr INT  := 1;

    v_flow_pk BIGINT;
    v_rowcount INT;
BEGIN
    RAISE NOTICE '================= FLOW ROLLBACK START =================';

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
