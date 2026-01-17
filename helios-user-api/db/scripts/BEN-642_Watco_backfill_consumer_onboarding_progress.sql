-- ============================================================================ 
-- 🚀 Script    : Backfill consumer_flow_progress and onboarding_progress_history
-- 📌 Purpose   : Iterates over all consumers for a tenant/cohort, ensuring progress 
--               records exist for the onboarding flow. Inserts missing records into 
--               `huser.consumer_flow_progress` and `huser.consumer_flow_progress_history` 
--               based on each consumer's `onboarding_state`.
-- 👨‍💻 Author   : Rakesh Pernati (refactored)
-- 📅 Date      : 2025-10-08
-- 🧾 Jira      : BEN-642
-- ⚠️ Inputs    : 
--      v_tenant_code  → Tenant identifier
-- 📤 Output    : 
--      - Inserted missing records in `huser.consumer_flow_progress`
--      - Inserted corresponding records in `huser.consumer_flow_progress_history`
--      - Logs for inserted, skipped, or errored consumers
-- 🔗 Script URL: <Optional Confluence / GitHub link>
-- 📝 Notes     : 
--      - Idempotent: safe to run multiple times
--      - Status set to 'IN_PROGRESS' unless onboarding_state = 'VERIFIED'
--      - Terminal step for 'VERIFIED' sets status = 'COMPLETED'
--      - Skips consumers if no matching flow step found
--      - Includes counters and structured logging
-- ============================================================================


DO $$
DECLARE
    v_tenant_code TEXT := '<WATCO-TENANT-CODE>';   -- Change the tenant code
    v_cohort_code TEXT := NULL;                 -- Change if needed
    v_flow_name   TEXT := 'onboarding_flow';    
    v_flow_pk BIGINT;
    v_version_nbr INT;
    v_consumer RECORD;
    v_progress_pk BIGINT;
    v_flow_step_pk BIGINT;
    v_status TEXT;

    -- counters
    v_count_total   INT := 0;
    v_count_insert  INT := 0;
    v_count_skip    INT := 0;
    v_count_error   INT := 0;
BEGIN
    RAISE NOTICE '▶️ Backfill started for tenant=% cohort=% flow=%',
        v_tenant_code, COALESCE(v_cohort_code,'<NULL>'), v_flow_name;

    -- 1. Find the flow for the tenant/cohort
    SELECT pk, version_nbr
    INTO v_flow_pk, v_version_nbr
    FROM tenant.flow
    WHERE tenant_code = v_tenant_code
      AND flow_name = v_flow_name
      AND delete_nbr = 0
      AND (
          (cohort_code = v_cohort_code)
          OR (cohort_code IS NULL AND v_cohort_code IS NULL)
      )
    LIMIT 1;

    IF v_flow_pk IS NULL THEN
        RAISE EXCEPTION '❌ No flow found for tenant %, cohort %, flow %',
            v_tenant_code, COALESCE(v_cohort_code,'<NULL>'), v_flow_name;
    ELSE
        RAISE NOTICE '✔️ Using flow % (pk=% version=%) for tenant %, cohort %',
            v_flow_name, v_flow_pk, v_version_nbr, v_tenant_code, COALESCE(v_cohort_code,'<NULL>');
    END IF;

    -- 2. Loop through all consumers in the tenant
    FOR v_consumer IN
        SELECT consumer_code, onboarding_state, agreement_status
        FROM huser.consumer
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
    LOOP
        v_count_total := v_count_total + 1;

        BEGIN
            v_flow_step_pk := NULL;
            v_progress_pk := NULL;
            v_status := 'in_progress';

            RAISE NOTICE '▶️ [%] Processing consumer % (state=%, agreement=%)',
                v_count_total, v_consumer.consumer_code,
                v_consumer.onboarding_state, v_consumer.agreement_status;

            -- ========== State → Flow Step Mapping ==========
            IF v_consumer.onboarding_state = 'VERIFIED' THEN
                v_status := 'completed';
                SELECT pk
                INTO v_flow_step_pk
                FROM tenant.flow_step
                WHERE flow_fk = v_flow_pk
                  AND on_success_component_catalogue_fk IS NULL
                  AND delete_nbr = 0
                LIMIT 1;
            ELSE
                SELECT fs.pk
                INTO v_flow_step_pk
                FROM tenant.flow_step fs
                WHERE fs.flow_fk = v_flow_pk
                  AND fs.delete_nbr = 0
                  AND fs.current_component_catalogue_fk = (
                      SELECT inner_fs.on_success_component_catalogue_fk
                      FROM tenant.flow_step inner_fs
                      JOIN tenant.component_catalogue cc 
                        ON inner_fs.current_component_catalogue_fk = cc.pk
                      WHERE inner_fs.flow_fk = v_flow_pk
                        AND inner_fs.delete_nbr = 0
                        AND cc.component_name = CASE v_consumer.onboarding_state
                              WHEN 'EMAIL_VERIFIED'       THEN 'email_verification_screen'
                              WHEN 'DOB_VERIFIED'         THEN 'dob_verification_screen'
                              WHEN 'CARD_LAST_4_VERIFIED' THEN 'card_last_4_verification_screen'
                              WHEN 'PICK_A_PURSE_COMPLETED' THEN 'pick_a_purse_screen'
                              WHEN 'COSTCO_ACTIONS_VISITED' THEN 'costco_actions_screen'
                              ELSE NULL
                          END
                      LIMIT 1
                  )
                LIMIT 1;
            END IF;

            -- If no step found and still IN_PROGRESS, skip
            IF v_flow_step_pk IS NULL AND v_status = 'in_progress' THEN
                v_count_skip := v_count_skip + 1;
                RAISE NOTICE '⚠️ Skipping consumer % (state=%): no matching flow step',
                    v_consumer.consumer_code, v_consumer.onboarding_state;
                CONTINUE;
            END IF;

            -- Check if progress already exists
            SELECT pk INTO v_progress_pk
            FROM huser.consumer_flow_progress
            WHERE consumer_code = v_consumer.consumer_code
              AND tenant_code = v_tenant_code
              AND flow_fk = v_flow_pk
              AND delete_nbr = 0
              AND (
                  (cohort_code = v_cohort_code)
                  OR (cohort_code IS NULL AND v_cohort_code IS NULL)
              )
            LIMIT 1;

            -- Insert or Skip
            IF v_progress_pk IS NULL THEN
                INSERT INTO huser.consumer_flow_progress
                    (consumer_code, tenant_code, cohort_code, flow_fk, version_nbr,
                     flow_step_pk, status, context_json,
                     create_ts, update_ts, create_user, update_user, delete_nbr)
                VALUES
                    (v_consumer.consumer_code, v_tenant_code, v_cohort_code, v_flow_pk, v_version_nbr,
                     v_flow_step_pk, v_status, '{}'::jsonb,
                     NOW(), NOW(), 'SYSTEM_BACKFILL', NULL, 0)
                RETURNING pk INTO v_progress_pk;

                v_count_insert := v_count_insert + 1;
                RAISE NOTICE '✔️ Inserted progress for consumer % (pk=%, state=%)',
                    v_consumer.consumer_code, v_progress_pk, v_consumer.onboarding_state;

                INSERT INTO huser.consumer_flow_progress_history
                    (consumer_flow_progress_fk, consumer_code, tenant_code, cohort_code,
                     flow_fk, version_nbr, from_flow_step_pk, to_flow_step_pk, outcome,
                     create_ts, update_ts, create_user, update_user, delete_nbr)
                VALUES
                    (v_progress_pk, v_consumer.consumer_code, v_tenant_code, v_cohort_code,
                     v_flow_pk, v_version_nbr, NULL, v_flow_step_pk, 'BACKFILL',
                     NOW(), NOW(), 'SYSTEM_BACKFILL', NULL, 0);

            ELSE
                v_count_skip := v_count_skip + 1;
                RAISE NOTICE '⚠️ Consumer % already has progress (pk=%), skipping',
                    v_consumer.consumer_code, v_progress_pk;
            END IF;

        EXCEPTION WHEN OTHERS THEN
            v_count_error := v_count_error + 1;
            RAISE WARNING '❌ Error for consumer % (state=%): %',
                v_consumer.consumer_code, v_consumer.onboarding_state, SQLERRM;
        END;
    END LOOP;

    RAISE NOTICE '🏁 Backfill completed: total=% inserted=% skipped=% errors=%',
        v_count_total, v_count_insert, v_count_skip, v_count_error;
END $$;