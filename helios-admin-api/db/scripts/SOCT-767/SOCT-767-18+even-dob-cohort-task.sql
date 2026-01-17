-- ============================================================================
-- This script performs the following operations:
-- 1. Creates a new cohort (if it doesn't already exist).
-- 2. Creates a task associated with the cohort (if not already present).
-- 3. Adds task details for the task and tenant combination (if not existing).
-- 4. Configures a task reward with specific reward settings (only if not present).
-- 5. Links the newly created (or existing) cohort to the task reward.
-- 
-- Each insert operation includes existence checks to prevent duplicates.
-- Informative notices are raised to indicate whether a record was inserted
-- or already existed and was reused.
-- ============================================================================

DO $$
DECLARE  
    v_cohort_name TEXT := 'adult18up+even_dob';
	v_task_name TEXT := 'Play Weekly Trivia';
	v_task_detail_name TEXT := 'Play Weekly Trivia';
    v_task_external_code TEXT := 'play_week_heal_triv';
    v_tenant_code TEXT := 'ten-153bd6c47ebe4673a75c71faa22b9eb6';
    v_task_type_code TEXT := 'tty-5c44328dce5a4b60ab79ab13e9253f27';
    v_task_reward_type_code TEXT := 'rtc-74a93c5f7ef44020a4314b49936c5955';
    v_reward_amount NUMERIC := 7;
	v_reward_type TEXT := 'SWEEPSTAKES_ENTRIES';

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
	
	--schedule related variables		
    v_year INT := 2025;
    v_start_date DATE := make_date(v_year, 1, 1);
    v_end_date DATE := make_date(v_year, 12, 31);
    v_current_start DATE := v_start_date;
    v_current_end DATE;
    v_schedule JSONB := '[]'::jsonb;

BEGIN
    BEGIN
		--cohort
		SELECT cohort_id INTO v_cohort_id
            FROM cohort.cohort
            WHERE cohort_name = v_cohort_name AND delete_nbr = 0;

        IF v_cohort_id IS NULL THEN
			v_cohort_code := 'coh-' || REPLACE(gen_random_uuid()::text, '-', '');

			INSERT INTO cohort.cohort (
				cohort_code, cohort_name, cohort_description, parent_cohort_id, cohort_rule, 
				create_ts, update_ts, create_user, update_user, delete_nbr, cohort_enabled
			)
			SELECT v_cohort_code, v_cohort_name, 'This is a cohort with person age 18+ and having odd date of birth', NULL, '{}'::jsonb,
				NOW(), NULL, 'SYSTEM', NULL, 0, true
			WHERE NOT EXISTS (
				SELECT 1 FROM cohort.cohort WHERE cohort_name = v_cohort_name AND delete_nbr = 0
			)
			RETURNING cohort_id INTO v_cohort_id;
            RAISE NOTICE 'Inserted into cohort.cohort: cohort_id=%, cohort_code=%', v_cohort_id, v_cohort_code;
        ELSE
            RAISE NOTICE 'Cohort already exists: cohort_id=%', v_cohort_id;
        END IF;

        SELECT task_type_id INTO v_task_type_id  
        FROM task.task_type
        WHERE task_type_code = v_task_type_code AND delete_nbr = 0
        LIMIT 1; 

        IF v_task_type_id IS NULL THEN  
            RAISE EXCEPTION 'task_type_code "%" not found', v_task_type_code;
        END IF;

		--task detail
		SELECT task_id INTO v_task_id
            FROM task.task
            WHERE task_name = v_task_name AND delete_nbr = 0;			

        IF v_task_id IS NULL THEN
			v_task_code := 'tsk-' || REPLACE(gen_random_uuid()::text, '-', '');

			INSERT INTO task.task (
				task_type_id, task_code, task_name, create_ts, update_ts, create_user, 
				update_user, delete_nbr, self_report, confirm_report, task_category_id, is_subtask
			)
			SELECT v_task_type_id, v_task_code, v_task_name, NOW(), NULL, 'SYSTEM', NULL, 0, false, false, NULL, false 
			WHERE NOT EXISTS (
				SELECT 1 FROM task.task WHERE task_name = v_task_name AND delete_nbr = 0
			)
			RETURNING task_id INTO v_task_id;
            RAISE NOTICE 'Inserted into task.task: task_id=%, task_code=%', v_task_id, v_task_code;
        ELSE
			RAISE NOTICE 'Task already exists: task_id=%', v_task_id;
        END IF;

		--task reward
		SELECT task_detail_id INTO v_task_detail_id
            FROM task.task_detail
            WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0;

        IF v_task_detail_id IS NULL THEN
			INSERT INTO task.task_detail (
				task_id, language_code, task_header, task_description, terms_of_service_id, 
				create_ts, update_ts, create_user, update_user, delete_nbr, 
				task_cta_button_text, tenant_code
			)
			SELECT v_task_id, 'en-US', v_task_detail_name, 'Sample task detail description', 1, 
				   NOW(), NULL, 'SYSTEM', NULL, 0, 'Play Now', v_tenant_code 
			WHERE NOT EXISTS (
				SELECT 1 FROM task.task_detail 
				WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0
			)
			RETURNING task_detail_id INTO v_task_detail_id;
            RAISE NOTICE 'Inserted task_detail for task_id=%', v_task_id;
        ELSE
			RAISE NOTICE 'Task detail already exists: task_detail_id=%', v_task_detail_id;
        END IF;

        SELECT reward_type_id INTO v_task_reward_type_id  
        FROM task.reward_type
        WHERE reward_type_code = v_task_reward_type_code
        LIMIT 1; 

        IF v_task_reward_type_id IS NULL THEN  
            RAISE EXCEPTION 'reward_type_code "%" not found', v_task_reward_type_code;
        END IF;

		--task reward
		SELECT task_reward_code INTO v_task_reward_code
            FROM task.task_reward 
            WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0;

        IF v_task_reward_code IS NULL THEN
			v_task_reward_code := 'trw-' || REPLACE(gen_random_uuid()::text, '-', '');
			
			--prepare daily shedule json config
			WHILE v_current_start <= v_end_date LOOP
				v_current_end := LEAST(v_current_start + 6, v_end_date);

				v_schedule := v_schedule || jsonb_build_object(
					'startDate', to_char(v_current_start, 'MM-DD'),
					'expiryDate', to_char(v_current_end, 'MM-DD')
				);

				v_current_start := v_current_start + 7;
			END LOOP;
		
			INSERT INTO task.task_reward (
				task_id, reward_type_id, tenant_code, task_reward_code, reward, min_task_duration, 
				max_task_duration, expiry, priority, create_ts, update_ts, create_user, 
				update_user, delete_nbr, task_action_url, task_external_code, valid_start_ts, 
				is_recurring, recurrence_definition_json, self_report, task_completion_criteria_json, 
				confirm_report, task_reward_config_json, is_collection
			)
			SELECT v_task_id, v_task_reward_type_id, v_tenant_code, v_task_reward_code, 
				jsonb_build_object('rewardType', v_reward_type, 'rewardAmount', v_reward_amount, 'membershipType', NULL), 
				0, 0, '2100-01-01 00:00:00', -10, NOW(), NULL, 'SYSTEM', NULL, 0, NULL, v_task_external_code, 
				'2025-01-01 00:00:00', true, 
				--'{"periodic": {"period": "MONTH","maxOccurrences": 1,"periodRestartDate": "1"},"recurrenceType": "PERIODIC"}'::jsonb, 
				jsonb_build_object(
					'schedules', v_schedule,
					'recurrenceType', 'SCHEDULE'
				),
				true, NULL, false, '{}'::jsonb, false 
			WHERE NOT EXISTS (
				SELECT 1 FROM task.task_reward 
				WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0
			)
			RETURNING task_reward_code INTO v_task_reward_code;
            RAISE NOTICE 'Inserted task_reward: task_reward_code=%', v_task_reward_code;
        ELSE
			RAISE NOTICE 'Task reward already exists: task_reward_code=%', v_task_reward_code;
        END IF;

		--cohort tenant task reward
		SELECT cohort_tenant_task_reward_id INTO v_cohort_tenant_task_reward_id
            FROM cohort.cohort_tenant_task_reward 
            WHERE cohort_id = v_cohort_id AND tenant_code = v_tenant_code 
            AND task_reward_code = v_task_reward_code AND delete_nbr = 0;

        IF v_cohort_tenant_task_reward_id IS NULL THEN
			INSERT INTO cohort.cohort_tenant_task_reward (
				cohort_id, tenant_code, task_reward_code, recommended, priority, 
				create_ts, update_ts, create_user, update_user, delete_nbr
			)
			SELECT v_cohort_id, v_tenant_code, v_task_reward_code, true, -10, 
				NOW(), NULL, 'SYSTEM', NULL, 0 
			WHERE NOT EXISTS (
				SELECT 1 FROM cohort.cohort_tenant_task_reward 
				WHERE cohort_id = v_cohort_id AND tenant_code = v_tenant_code 
				AND task_reward_code = v_task_reward_code AND delete_nbr = 0
			)
			RETURNING cohort_tenant_task_reward_id INTO v_cohort_tenant_task_reward_id;
            RAISE NOTICE 'Linked cohort to task_reward successfully. cohort_tenant_task_reward_id=%', v_cohort_tenant_task_reward_id;
        ELSE
            RAISE NOTICE 'Link between cohort and task_reward already exists. cohort_tenant_task_reward_id=%', v_cohort_tenant_task_reward_id;
        END IF;

        RAISE NOTICE 'Script completed successfully.';

    EXCEPTION
        WHEN OTHERS THEN
            RAISE NOTICE 'Error occurred: %', SQLERRM;
            RAISE EXCEPTION 'Transaction rolled back due to error.';
    END;
END $$;
