-- ============================================================================
-- 🚀 Script    : This script performs the following operations:
-- 				   1. Creates a new cohort (if it doesn't already exist).
-- 				   2. Creates a task associated with the cohort (if not already present).
-- 				   3. Adds task details for the task and tenant combination (if not existing).
-- 				   4. Configures a task reward with specific reward settings (only if not present).
-- 				   5. Links the newly created (or existing) cohort to the task reward.
-- 📌 Purpose   : Insert cohort and task for sweepstakes_winners task
-- 🧑 Author    : Kumar Sirikonda / Siva Krishna 
-- 📅 Date      : 04-12-2025
-- 🧾 Jira      : RES-928
-- ⚠️ Inputs    : Array of Navitus 2026 Tenants
-- 📤 Output    : Each insert operation includes existence checks to prevent duplicates.
-- 				  Informative notices are raised to indicate whether a record was inserted
-- 				  or already existed and was reused.
-- 🔗 Script URL: NA
-- 📝 Notes     : Execute the script only for NAVITUS 2026 Tenants.
-- ============================================================================
DO $$
DECLARE  
	--Note: Execute the script only for Navitus 2026 tenants(ten-bf640c1db734468a9ec9navdbd0fc348) 
    v_tenant_codes TEXT[] := ARRAY[
        '<NAVITUS-2026-TENANT>',
        '<NAVITUS-2026-TENANT>'
    ];

	--loop variable
    v_tenant TEXT;

	--variables
    v_cohort_name TEXT := 'sweepstakes_winner';
	v_task_name TEXT := 'sweepstakes_winners';
	v_task_detail_name TEXT := 'Sweepstakes Winner';
    v_task_external_code TEXT := 'swee_winn';
    v_task_type_code TEXT := 'tty-dc2638e6ba2c47d4be6921e68175f43d';
    v_task_reward_type_code TEXT := 'rtc-a5a943d3fc2a4506ab12218204d60805';
    v_reward_amount NUMERIC := 100;
	v_reward_type TEXT := 'MONETARY_DOLLARS';

	--Auto generated
    v_cohort_id BIGINT;
    v_cohort_code TEXT;
    v_task_code TEXT;
    v_task_type_id BIGINT;
    v_task_id BIGINT;
	v_task_detail_id BIGINT;
    v_task_reward_type_id BIGINT;
	v_task_reward_id BIGINT;
    v_task_reward_code TEXT;
	v_cohort_tenant_task_reward_id BIGINT;

    --schedule (unused, but kept unchanged)
    v_year INT := 2025;
    v_start_date DATE := make_date(v_year, 1, 1);
    v_end_date DATE := make_date(v_year, 12, 31);
    v_current_start DATE := v_start_date;
    v_current_end DATE;
    v_schedule JSONB := '[]'::jsonb;

BEGIN
    BEGIN
		-- ============================================================
		-- COHORT (runs once — NOT tenant specific)
		-- ============================================================
		SELECT cohort_id INTO v_cohort_id
        FROM cohort.cohort
        WHERE cohort_name = v_cohort_name AND delete_nbr = 0;

        IF v_cohort_id IS NULL THEN
			v_cohort_code := 'coh-' || REPLACE(gen_random_uuid()::text, '-', '');

			INSERT INTO cohort.cohort (
				cohort_code, cohort_name, cohort_description, parent_cohort_id, cohort_rule, 
				create_ts, update_ts, create_user, update_user, delete_nbr, cohort_enabled
			)
			SELECT v_cohort_code, v_cohort_name, 'This is a cohort for sweepstakes winnera', NULL, '{}'::jsonb,
				NOW(), NULL, 'SYSTEM', NULL, 0, true
			WHERE NOT EXISTS (
				SELECT 1 FROM cohort.cohort WHERE cohort_name = v_cohort_name AND delete_nbr = 0
			)
			RETURNING cohort_id INTO v_cohort_id;

            RAISE NOTICE 'Inserted cohort: %', v_cohort_id;
        ELSE
            RAISE NOTICE 'Cohort already exists: %', v_cohort_id;
        END IF;

		-- ============================================================
		-- TASK TYPE
		-- ============================================================
        SELECT task_type_id INTO v_task_type_id  
        FROM task.task_type
        WHERE task_type_code = v_task_type_code AND delete_nbr = 0
        LIMIT 1;

        IF v_task_type_id IS NULL THEN  
            RAISE EXCEPTION 'task_type_code "%" not found', v_task_type_code;
        END IF;

		-- ============================================================
		-- TASK (runs once — NOT tenant specific)
		-- ============================================================
		SELECT task_id INTO v_task_id
        FROM task.task
        WHERE task_name = v_task_name AND delete_nbr = 0;

        IF v_task_id IS NULL THEN
			v_task_code := 'tsk-' || REPLACE(gen_random_uuid()::text, '-', '');

			INSERT INTO task.task (
				task_type_id, task_code, task_name, create_ts, update_ts, create_user, 
				update_user, delete_nbr, self_report, confirm_report, task_category_id, is_subtask
			)
			SELECT v_task_type_id, v_task_code, v_task_name, NOW(), NULL, 'SYSTEM', NULL, 0,
			       false, false, NULL, false 
			WHERE NOT EXISTS (
				SELECT 1 FROM task.task WHERE task_name = v_task_name AND delete_nbr = 0
			)
			RETURNING task_id INTO v_task_id;

            RAISE NOTICE 'Inserted task: %', v_task_id;
        ELSE
			Raise notice 'Task already exists: %', v_task_id;
        END IF;

		-- ============================================================
		-- TASK REWARD TYPE
		-- ============================================================
        SELECT reward_type_id INTO v_task_reward_type_id  
        FROM task.reward_type
        WHERE reward_type_code = v_task_reward_type_code
        LIMIT 1;

        IF v_task_reward_type_id IS NULL THEN  
            RAISE EXCEPTION 'reward_type_code "%" not found', v_task_reward_type_code;
        END IF;

		-- ============================================================
		-- PROCESS EACH TENANT
		-- ============================================================
		FOREACH v_tenant IN ARRAY v_tenant_codes LOOP

			RAISE NOTICE 'Processing tenant: %', v_tenant;

			-- task_detail (tenant-specific)
			SELECT task_detail_id INTO v_task_detail_id
			FROM task.task_detail
			WHERE task_id = v_task_id AND tenant_code = v_tenant AND delete_nbr = 0;

			IF v_task_detail_id IS NULL THEN
				INSERT INTO task.task_detail (
					task_id, language_code, task_header, task_description, terms_of_service_id, 
					create_ts, update_ts, create_user, update_user, delete_nbr, 
					task_cta_button_text, tenant_code
				)
				SELECT v_task_id, 'en-US', v_task_detail_name, 
				       'Sample task detail description for sweepstakes winners', 1, 
					   NOW(), NULL, 'SYSTEM', NULL, 0, 'Get started', v_tenant
				WHERE NOT EXISTS (
					SELECT 1 FROM task.task_detail 
					WHERE task_id = v_task_id AND tenant_code = v_tenant AND delete_nbr = 0
				)
				RETURNING task_detail_id INTO v_task_detail_id;

				RAISE NOTICE 'Inserted task_detail for tenant %, task_detail_id=%', v_tenant, v_task_detail_id;
			ELSE
				RAISE NOTICE 'Task detail exists for %, id=%', v_tenant, v_task_detail_id;
			END IF;

			-- task_reward (tenant-specific)
			SELECT task_reward_code INTO v_task_reward_code
			FROM task.task_reward 
			WHERE task_id = v_task_id AND tenant_code = v_tenant AND delete_nbr = 0;

			IF v_task_reward_code IS NULL THEN
				v_task_reward_code := 'trw-' || REPLACE(gen_random_uuid()::text, '-', '');

				INSERT INTO task.task_reward (
					task_id, reward_type_id, tenant_code, task_reward_code, reward, 
					min_task_duration, max_task_duration, expiry, priority, create_ts, 
					update_ts, create_user, update_user, delete_nbr, task_action_url, 
					task_external_code, valid_start_ts, is_recurring, 
					recurrence_definition_json, self_report, task_completion_criteria_json, 
					confirm_report, task_reward_config_json, is_collection
				)
				SELECT 
					v_task_id, v_task_reward_type_id, v_tenant, v_task_reward_code,
					jsonb_build_object('rewardType', v_reward_type, 'rewardAmount', v_reward_amount, 'membershipType', NULL),
					0, 0, '2100-01-01 00:00:00', -10, NOW(), NULL, 'SYSTEM', NULL, 0,
					NULL, v_task_external_code, '2025-01-01 00:00:00',
					true, '{}'::jsonb, false, NULL, false, '{}'::jsonb, false
				WHERE NOT EXISTS (
					SELECT 1 FROM task.task_reward 
					WHERE task_id = v_task_id AND tenant_code = v_tenant AND delete_nbr = 0
				)
				RETURNING task_reward_code INTO v_task_reward_code;

				RAISE NOTICE 'Inserted task_reward for tenant %, code=%', v_tenant, v_task_reward_code;
			ELSE
				RAISE NOTICE 'Task reward exists for tenant %, code=%', v_tenant, v_task_reward_code;
			END IF;

			-- cohort_tenant_task_reward
			SELECT cohort_tenant_task_reward_id INTO v_cohort_tenant_task_reward_id
			FROM cohort.cohort_tenant_task_reward 
			WHERE cohort_id = v_cohort_id 
			  AND tenant_code = v_tenant
			  AND task_reward_code = v_task_reward_code
			  AND delete_nbr = 0;

			IF v_cohort_tenant_task_reward_id IS NULL THEN
				INSERT INTO cohort.cohort_tenant_task_reward (
					cohort_id, tenant_code, task_reward_code, recommended, priority,
					create_ts, update_ts, create_user, update_user, delete_nbr
				)
				SELECT v_cohort_id, v_tenant, v_task_reward_code, true, -10, 
				       NOW(), NULL, 'SYSTEM', NULL, 0
				WHERE NOT EXISTS (
					SELECT 1 FROM cohort.cohort_tenant_task_reward 
					WHERE cohort_id = v_cohort_id AND tenant_code = v_tenant
					AND task_reward_code = v_task_reward_code AND delete_nbr = 0
				)
				RETURNING cohort_tenant_task_reward_id INTO v_cohort_tenant_task_reward_id;

				RAISE NOTICE 'Linked cohort to reward for tenant %, id=%', v_tenant, v_cohort_tenant_task_reward_id;
			ELSE
				RAISE NOTICE 'Link exists for tenant %, id=%', v_tenant, v_cohort_tenant_task_reward_id;
			END IF;

		END LOOP;

		RAISE NOTICE 'Script completed successfully for all tenants.';

    EXCEPTION
        WHEN OTHERS THEN
            RAISE NOTICE 'Error: %', SQLERRM;
            RAISE EXCEPTION 'Transaction rolled back.';
    END;
END $$;
