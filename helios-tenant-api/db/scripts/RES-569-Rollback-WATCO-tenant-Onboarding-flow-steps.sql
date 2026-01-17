-- ============================================================================
-- 🚀 Script    : Rollback of onboarding flow for agreement screen
-- 📌 Purpose   : Updates onbaarding flow  under
--                a specific tenant by mapping tenant name to flow steps.
-- 🧑 Author    : KAWALPREET KAUR
-- 📅 Date      : 2025-10-06
-- 🧾 Jira      : RES-569
-- ⚠️ Inputs    : v_tenant_code (TEXT) 
-- 📤 Output    : Success notice.
-- 🔗 Script URL: https://github.com/SunnyRewards/helios-tenant-api/blob/develop/db/scripts/RES-569-Rollback-WATCO-tenant-Onboarding-flow-steps.sql
-- ============================================================================


DO $$
DECLARE
    -- ========= INPUT PARAMETERS =========
    v_tenant_code TEXT := '<WATCO-TENANT-CODE>';     -- <<<<<< INPUT WATCO-TENANT-CODE
    v_cohort_code TEXT := NULL;                   -- <<<<<< CHANGE COHORT CODE (nullable allowed)
    v_flow_name   TEXT := 'onboarding_flow';
    v_version_nbr INT  := 1;                      -- Input Version Number 
	
	-- INPUT Array of FLOW steps for WATCO-TENANT (ORDER MATTERS!)
    v_flowSteps TEXT[] := ARRAY[
		'rewards_splash_screen'
    ];
	
    v_effective_start_ts TIMESTAMPTZ := NOW();
    v_effective_end_ts   TIMESTAMPTZ := NULL;

    -- ========= INTERNAL VARIABLES =========
    v_flow_pk BIGINT;
    v_step_pk BIGINT;
	v_rowcount INT;

    v_step_idx INT;
    v_current_component BIGINT;
    v_next_component BIGINT;
    v_name TEXT;
BEGIN
    RAISE NOTICE '================= FLOW CREATION START =================';

    -- =============================================
    -- FLOW (Insert or Update)
    -- =============================================
    SELECT pk INTO v_flow_pk
    FROM tenant.flow
    WHERE tenant_code = v_tenant_code
      AND (cohort_code = v_cohort_code OR (v_cohort_code IS NULL AND cohort_code IS NULL))
      AND flow_name = v_flow_name
      AND version_nbr = v_version_nbr
      AND delete_nbr = 0;

    IF v_flow_pk IS NULL THEN
        INSERT INTO tenant.flow (
            tenant_code, cohort_code, flow_name, version_nbr, effective_start_ts, effective_end_ts,
            create_ts, update_ts, create_user, update_user, delete_nbr
        )
        VALUES (
            v_tenant_code, v_cohort_code, v_flow_name, v_version_nbr, 
            v_effective_start_ts, v_effective_end_ts,
            NOW(), NOW(), 'SYSTEM', NULL, 0
        )
        RETURNING pk INTO v_flow_pk;

        RAISE NOTICE '🎉 Inserted new flow "%", pk = %', v_flow_name, v_flow_pk;
    ELSE
        UPDATE tenant.flow
        SET effective_start_ts = v_effective_start_ts,
            effective_end_ts   = v_effective_end_ts,
            update_user        = 'SYSTEM',
            update_ts          = NOW()
        WHERE pk = v_flow_pk;

        RAISE NOTICE '✏️ Updated existing flow "%" (pk = %), effective_start_ts = %, effective_end_ts = %',
            v_flow_name, v_flow_pk, v_effective_start_ts, v_effective_end_ts;
    END IF;

    -- =============================================
    -- FLOW STEPS (Upsert by step_idx only)
    -- =============================================
    FOR v_step_idx IN 1..array_length(v_flowSteps, 1) LOOP
        v_name := v_flowSteps[v_step_idx];

        -- Get current component pk
        SELECT pk INTO v_current_component
        FROM tenant.component_catalogue
        WHERE component_name = v_name
          AND delete_nbr = 0;

        IF v_current_component IS NULL THEN
            RAISE EXCEPTION '❌ Component "%" not found in tenant.component_catalogue', v_name;
        END IF;

        -- Get next step component pk (if exists in array)
        IF v_step_idx < array_length(v_flowSteps, 1) THEN
            SELECT pk INTO v_next_component
            FROM tenant.component_catalogue
            WHERE component_name = v_flowSteps[v_step_idx + 1]
              AND delete_nbr = 0;
        ELSE
            v_next_component := NULL;
        END IF;

        -- Try update first by step_idx
        UPDATE tenant.flow_step
        SET current_component_catalogue_fk    = v_current_component,
            on_success_component_catalogue_fk = v_next_component,
            on_failure_component_catalogue_fk = v_current_component,
            update_ts = NOW(),
            update_user = 'SYSTEM',
            delete_nbr = 0
        WHERE flow_fk = v_flow_pk
          AND step_idx = v_step_idx
        RETURNING pk INTO v_step_pk;

        -- If nothing updated, insert new
        IF v_step_pk IS NULL THEN
            INSERT INTO tenant.flow_step (
                flow_fk, step_idx, current_component_catalogue_fk, 
                on_success_component_catalogue_fk, on_failure_component_catalogue_fk,
                create_ts, update_ts, create_user, update_user, delete_nbr
            )
            VALUES (
                v_flow_pk, v_step_idx, v_current_component,
                v_next_component, v_current_component,  -- success → next, failure → same
                NOW(), NULL, 'SYSTEM', NULL, 0
            )
            RETURNING pk INTO v_step_pk;

            RAISE NOTICE '✅ Inserted flow_step % ("%") pk = %', v_step_idx, v_name, v_step_pk;
        ELSE
            RAISE NOTICE '♻️ Updated flow_step % ("%") pk = %', v_step_idx, v_name, v_step_pk;
        END IF;
    END LOOP;

    -- =============================================
    -- CLEANUP (Mark extra steps beyond array length as deleted)
    -- =============================================
	UPDATE tenant.flow_step
	       SET delete_nbr = 1,
	           update_ts  = NOW(),
	           update_user = 'SYSTEM'
	     WHERE flow_fk = v_flow_pk
	       AND step_idx > array_length(v_flowSteps, 1)
	       AND delete_nbr = 0;
	
	    GET DIAGNOSTICS v_rowcount = ROW_COUNT;
	    IF v_rowcount > 0 THEN
	        RAISE NOTICE '🧹 Cleanup: % extra step(s) marked deleted for flow_pk = %', v_rowcount, v_flow_pk;
	    END IF;
	
	    RAISE NOTICE '================= FLOW CREATION COMPLETE =================';
END $$;