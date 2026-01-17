-- ============================================================================
-- 📌 Purpose   : 
--   - Add/Update onboarding_flow configuration for HAP-related cohorts
--   - Ensures flow metadata exists in tenant.flow for each cohort
--   - Ensures ordered flow steps exist in tenant.flow_step for each flow
--   - Marks any extra flow steps as deleted (cleanup)
-- 🧑 Author    : Kawalpreet Kaur
-- 📅 Date      : 2025-09-25
-- 🧾 Jira      : BEN-569
-- ⚠️ Inputs    : 
--       v_tenant_code  (HAP tenant code, required)
--       v_cohort_code  (Looked up dynamically by cohort_name if not provided)
--       v_flow_name    ('onboarding_flow')
--       v_version_nbr  (Version number of the flow, e.g., 2)
--       v_flowSteps    (Array of ordered component names per cohort)
--          Cohort "healthy_living_rewards": 
--              1. rewards_splash_screen
--              2. agreement_screen
--              3. notification_screen
--          Cohort "hap_flex_rewards" and "henry_ford_health_flex_rewards":
--              1. rewards_splash_screen
--              2. agreement_screen
--              3. activate_card_model
--              4. dob_verification_screen
--              5. card_last_4_verification_screen
--              6. card_activate_success_model
--              7. notification_screen
-- 📤 Output    : 
--       - Inserts new flow if not existing, else updates effective dates
--       - Inserts or updates each flow_step (success → next, failure → self)
--       - Logs detailed notices for insert/update actions
--       - Marks extra/unwanted flow_steps as deleted
-- 🔗 Script URL: https://github.com/SunnyRewards/helios-tenant-api/blob/develop/db/scripts/RES-569-Rollback-HAP-tenant-Onboarding-flow-steps.sql
-- 📝 Notes     : 
--   - Idempotent: can be re-run safely without duplicating flows/steps
--   - Relies on tenant.component_catalogue for component existence
--   - Uses SYSTEM as create/update_user, delete_nbr = 0 for active rows
-- ============================================================================


-- healthy_living_rewards cohort
DO $$
DECLARE
    -- ========= INPUT PARAMETERS =========
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';     -- <<<<<< INPUT HAP TENANT CODE
    v_cohort_code TEXT := NULL;                   -- <<<<<< CHANGE COHORT CODE (nullable allowed)
    v_flow_name   TEXT := 'onboarding_flow';
    v_version_nbr INT  := 1;                      -- Input Version Number 

	-- INPUT Array of FLOW steps for HAP-TENANT (ORDER MATTERS!)
    v_flowSteps TEXT[] := ARRAY[
        'rewards_splash_screen',
        'agreement_screen',
        'notification_screen'
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

    ----------------------------------------------------------------------
    -- Look up cohort_code from cohort.cohort table by cohort_name
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


    -- =============================================
    -- FLOW (Insert or Update)
    -- =============================================
    SELECT pk INTO v_flow_pk
    FROM tenant.flow
    WHERE tenant_code = v_tenant_code
      AND cohort_code = v_cohort_code 
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
-- ============================================================================
-- 📌 Purpose   : 
--   - Add/Update onboarding_flow configuration for HAP-related cohorts
--   - Ensures flow metadata exists in tenant.flow for each cohort
--   - Ensures ordered flow steps exist in tenant.flow_step for each flow
--   - Marks any extra flow steps as deleted (cleanup)
-- 🧑 Author    : Kawalpreet Kaur
-- 📅 Date      : 2025-09-25
-- 🧾 Jira      : RES-569
-- ⚠️ Inputs    : 
--       v_tenant_code  (HAP tenant code, required)
--       v_cohort_code  (Looked up dynamically by cohort_name if not provided)
--       v_flow_name    ('onboarding_flow')
--       v_version_nbr  (Version number of the flow, e.g., 2)
--       v_flowSteps    (Array of ordered component names per cohort)
--          Cohort "healthy_living_rewards": 
--              1. rewards_splash_screen
--              2. agreement_screen
--              3. notification_screen
--          Cohort "hap_flex_rewards" and "henry_ford_health_flex_rewards":
--              1. rewards_splash_screen
--              2. agreement_screen
--              3. activate_card_model
--              4. dob_verification_screen
--              5. card_last_4_verification_screen
--              6. card_activate_success_model
--              7. notification_screen
-- 📤 Output    : 
--       - Inserts new flow if not existing, else updates effective dates
--       - Inserts or updates each flow_step (success → next, failure → self)
--       - Logs detailed notices for insert/update actions
--       - Marks extra/unwanted flow_steps as deleted
-- 🔗 Script URL: https://github.com/SunnyRewards/helios-tenant-api/blob/develop/db/scripts/RES-569-Rollback-HAP-tenant-Onboarding-flow-steps.sql
-- 📝 Notes     : 
--   - Idempotent: can be re-run safely without duplicating flows/steps
--   - Relies on tenant.component_catalogue for component existence
--   - Uses SYSTEM as create/update_user, delete_nbr = 0 for active rows
-- ============================================================================
-- hap_flex_rewards cohort
DO $$
DECLARE
    -- ========= INPUT PARAMETERS =========
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';     -- <<<<<< INPUT HAP TENANT CODE
    v_cohort_code TEXT := NULL;                   -- <<<<<< CHANGE COHORT CODE (nullable allowed)
    v_flow_name   TEXT := 'onboarding_flow';
    v_version_nbr INT  := 1;                      -- Input Version Number 

	-- INPUT Array of FLOW steps for HAP-TENANT (ORDER MATTERS!)
    v_flowSteps TEXT[] := ARRAY[
        'rewards_splash_screen',
        'agreement_screen',
	    'activate_card_model',
        'dob_verification_screen',
        'card_last_4_verification_screen',
	    'card_activate_success_model',
        'notification_screen'
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

    ----------------------------------------------------------------------
    -- Look up cohort_code from cohort.cohort table by cohort_name
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


    -- =============================================
    -- FLOW (Insert or Update)
    -- =============================================
    SELECT pk INTO v_flow_pk
    FROM tenant.flow
    WHERE tenant_code = v_tenant_code
      AND cohort_code = v_cohort_code
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
-- ============================================================================
-- 📌 Purpose   : 
--   - Add/Update onboarding_flow configuration for HAP-related cohorts
--   - Ensures flow metadata exists in tenant.flow for each cohort
--   - Ensures ordered flow steps exist in tenant.flow_step for each flow
--   - Marks any extra flow steps as deleted (cleanup)
-- 🧑 Author    : Kawalpreet Kaur
-- 📅 Date      : 2025-09-25
-- 🧾 Jira      : Res-569
-- ⚠️ Inputs    : 
--       v_tenant_code  (HAP tenant code, required)
--       v_cohort_code  (Looked up dynamically by cohort_name if not provided)
--       v_flow_name    ('onboarding_flow')
--       v_version_nbr  (Version number of the flow, e.g., 2)
--       v_flowSteps    (Array of ordered component names per cohort)
--          Cohort "healthy_living_rewards": 
--              1. rewards_splash_screen
--              2. agreement_screen
--              3. notification_screen
--          Cohort "hap_flex_rewards" and "henry_ford_health_flex_rewards":
--              1. rewards_splash_screen
--              2. agreement_screen
--              3. activate_card_model
--              4. dob_verification_screen
--              5. card_last_4_verification_screen
--              6. card_activate_success_model
--              7. notification_screen
-- 📤 Output    : 
--       - Inserts new flow if not existing, else updates effective dates
--       - Inserts or updates each flow_step (success → next, failure → self)
--       - Logs detailed notices for insert/update actions
--       - Marks extra/unwanted flow_steps as deleted
-- 🔗 Script URL: https://github.com/SunnyRewards/helios-tenant-api/blob/develop/db/scripts/RES-569-Rollback-HAP-tenant-Onboarding-flow-steps.sql
-- 📝 Notes     : 
--   - Idempotent: can be re-run safely without duplicating flows/steps
--   - Relies on tenant.component_catalogue for component existence
--   - Uses SYSTEM as create/update_user, delete_nbr = 0 for active rows
-- ============================================================================
-- henry_ford_health_flex_rewards cohort
DO $$
DECLARE
    -- ========= INPUT PARAMETERS =========
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';     -- <<<<<< INPUT HAP TENANT CODE
    v_cohort_code TEXT := NULL;                   -- <<<<<< CHANGE COHORT CODE (nullable allowed)
    v_flow_name   TEXT := 'onboarding_flow';
    v_version_nbr INT  := 1;                      -- Input Version Number 

	-- INPUT Array of FLOW steps for HAP-TENANT (ORDER MATTERS!)
    v_flowSteps TEXT[] := ARRAY[
         'rewards_splash_screen',
        'agreement_screen',
	    'activate_card_model',
        'dob_verification_screen',
        'card_last_4_verification_screen',
	    'card_activate_success_model',
        'notification_screen'
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

    ----------------------------------------------------------------------
    -- Look up cohort_code from cohort.cohort table by cohort_name
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


    -- =============================================
    -- FLOW (Insert or Update)
    -- =============================================
    SELECT pk INTO v_flow_pk
    FROM tenant.flow
    WHERE tenant_code = v_tenant_code
      AND cohort_code = v_cohort_code 
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
