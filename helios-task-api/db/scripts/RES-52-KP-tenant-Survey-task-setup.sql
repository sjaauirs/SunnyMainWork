-- =================================================================================================================================
-- 🚀 Script    : Script for insertion and updation of your voice matters task 
-- 📌 Purpose   : Introduces or updates "Your Voice Matters" task for KP tenants only.
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-10-07
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-52
-- ⚠️ Inputs    : KP-TENANT-CODE
-- 📤 Output    : Successfully updated or Inserted
-- 🔗 Script URL: NA
-- 📝 Notes     : Scripts needs to be executed in sequence. This is only for KP tenant script.
-- 🔢 Sequence Number:1
-- ===================================================================================================================================
DO $$
DECLARE  
    -- <Input Parameters>
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; --- KP tenant only	

    -- <Variable Declarations>
    v_cohort_name TEXT := 'Survey';
	v_task_name TEXT := 'Your Voice Matters';
	v_task_detail_name_en TEXT := 'Your voice matters';
	v_task_detail_name_es TEXT := 'Su voz importa';
    v_task_external_code TEXT := 'your_voic_matt';
    v_task_type_code TEXT := 'tty-86398dc3a77d4a3db7922e57b5b6d73c';
    v_task_reward_type_code TEXT := 'rtc-a5a943d3fc2a4506ab12218204d60805';
    v_reward_amount NUMERIC := 10;
	v_reward_type TEXT := 'MONETARY_DOLLARS';

    -- English task_detail description
    v_task_desc_en JSONB := '[{"type":"paragraph","data":{"text":"Help us shape the future of Kaiser Permanente rewards. This short survey will only take a few minutes and you''ll get $10 for sharing your thoughts."}}]'::jsonb;
    v_task_cta_en TEXT := 'Get started';
	-- Spanish task_detail description
    v_task_desc_es JSONB := '[{"type":"paragraph","data":{"text":"Ayúdenos a dar forma al futuro de las recompensas de Kaiser Permanente. Esta breve encuesta solo le llevará unos minutos y recibirá $10 por compartir sus opiniones."}}]'::jsonb;
    v_task_cta_es TEXT := 'Empezar';
 
	--Auto generated
    v_cohort_id BIGINT;
    v_cohort_code TEXT;
    v_task_code TEXT;
    v_task_type_id BIGINT;
    v_task_id BIGINT;
    v_task_reward_type_id BIGINT;
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
		-- Step 1: Cohort setup
		SELECT cohort_id INTO v_cohort_id
        FROM cohort.cohort
        WHERE cohort_name = v_cohort_name AND delete_nbr = 0;

        IF v_cohort_id IS NULL THEN
			v_cohort_code := 'coh-' || REPLACE(gen_random_uuid()::text, '-', '');
			INSERT INTO cohort.cohort (
				cohort_code, cohort_name, cohort_description, parent_cohort_id, cohort_rule, 
				create_ts, update_ts, create_user, update_user, delete_nbr, cohort_enabled
			)
			SELECT v_cohort_code, v_cohort_name, 'This is a cohort for persons eligible for survey.', NULL, '{}'::jsonb,
				NOW(), NULL, 'SYSTEM', NULL, 0, true
			WHERE NOT EXISTS (
				SELECT 1 FROM cohort.cohort WHERE cohort_name = v_cohort_name AND delete_nbr = 0
			)
			RETURNING cohort_id INTO v_cohort_id;
            RAISE NOTICE '[INFO]: Inserted cohort: %', v_cohort_id;
        ELSE
            RAISE NOTICE '[INFO]: Cohort exists: %', v_cohort_id;
        END IF;

        -- Step 2: Validate task_type
        SELECT task_type_id INTO v_task_type_id  
        FROM task.task_type
        WHERE task_type_code = v_task_type_code AND delete_nbr = 0
        LIMIT 1;

        IF v_task_type_id IS NULL THEN  
            RAISE EXCEPTION '[ERROR]: task_type_code "%" not found', v_task_type_code;
        END IF;

        -- Step 3: Create or find task
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
            RAISE NOTICE '[INFO]: Inserted task: %', v_task_id;
        ELSE
			RAISE NOTICE '[INFO]: Task exists: %', v_task_id;
        END IF;

        -- Step 4: Handle English task_detail (Insert or Update)
        IF NOT EXISTS (
            SELECT 1 FROM task.task_detail 
            WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND language_code='en-US' AND delete_nbr = 0
        ) THEN
            INSERT INTO task.task_detail (
                task_id, language_code, task_header, task_description, terms_of_service_id,
                create_ts, update_ts, create_user, update_user, delete_nbr,
                task_cta_button_text, tenant_code
            )
            VALUES (
                v_task_id, 'en-US', v_task_detail_name_en, v_task_desc_en, 1,
                NOW(), NULL, 'SYSTEM', NULL, 0,
                v_task_cta_en, v_tenant_code
            );
            RAISE NOTICE '[INFO]: Inserted English task_detail.';
        ELSE
            UPDATE task.task_detail
            SET task_header = v_task_detail_name_en,
                task_description = v_task_desc_en,
                task_cta_button_text = v_task_cta_en,
                update_ts = NOW(),
                update_user = 'SYSTEM'
            WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND language_code='en-US' AND delete_nbr = 0;
            RAISE NOTICE '[INFO]: Updated English task_detail.';
        END IF;

        -- Step 5: Handle Spanish task_detail (Insert or Update)
        IF NOT EXISTS (
            SELECT 1 FROM task.task_detail 
            WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND language_code='es' AND delete_nbr = 0
        ) THEN
            INSERT INTO task.task_detail (
                task_id, language_code, task_header, task_description, terms_of_service_id,
                create_ts, update_ts, create_user, update_user, delete_nbr,
                task_cta_button_text, tenant_code
            )
            VALUES (
                v_task_id, 'es', v_task_detail_name_es, v_task_desc_es, 1,
                NOW(), NULL, 'SYSTEM', NULL, 0,
                v_task_cta_es, v_tenant_code
            );
            RAISE NOTICE '[INFO]: Inserted Spanish task_detail.';
        ELSE
            UPDATE task.task_detail
            SET task_header = v_task_detail_name_es,
                task_description = v_task_desc_es,
                task_cta_button_text = v_task_cta_es,
                update_ts = NOW(),
                update_user = 'SYSTEM'
            WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND language_code='es' AND delete_nbr = 0;
            RAISE NOTICE '[INFO]: Updated Spanish task_detail.';
        END IF;

        -- Step 6: Task Reward setup
        SELECT reward_type_id INTO v_task_reward_type_id  
        FROM task.reward_type
        WHERE reward_type_code = v_task_reward_type_code
        LIMIT 1;

        IF v_task_reward_type_id IS NULL THEN  
            RAISE EXCEPTION '[ERROR]: reward_type_code "%" not found', v_task_reward_type_code;
        END IF;

        SELECT task_reward_code INTO v_task_reward_code
        FROM task.task_reward 
        WHERE tenant_code = v_tenant_code AND delete_nbr = 0 AND task_external_code = v_task_external_code;

        IF v_task_reward_code IS NULL THEN
			v_task_reward_code := 'trw-' || REPLACE(gen_random_uuid()::text, '-', '');
			WHILE v_current_start <= v_end_date LOOP
				v_current_end := LEAST(v_current_start + 6, v_end_date);
				v_schedule := v_schedule || jsonb_build_object('startDate', to_char(v_current_start, 'MM-DD'),'expiryDate', to_char(v_current_end, 'MM-DD'));
				v_current_start := v_current_start + 7;
			END LOOP;

			INSERT INTO task.task_reward (
				task_id, reward_type_id, tenant_code, task_reward_code, reward,
				min_task_duration, max_task_duration, expiry, priority, create_ts,
				update_ts, create_user, update_user, delete_nbr, task_action_url,
				task_external_code, valid_start_ts, is_recurring, recurrence_definition_json,
				self_report, task_completion_criteria_json, confirm_report,
				task_reward_config_json, is_collection
			)
			SELECT v_task_id, v_task_reward_type_id, v_tenant_code, v_task_reward_code, 
				jsonb_build_object('rewardType', v_reward_type, 'rewardAmount', v_reward_amount, 'membershipType', 'MONETARY_DOLLARS'),
				0, 0, '2100-01-01 00:00:00', -10, NOW(),
				NULL, 'SYSTEM', NULL, 0, NULL, v_task_external_code, 
				'2025-01-01 00:00:00', true,
				'{"periodic": {"period": "MONTH","maxOccurrences": 1,"periodRestartDate": "1"},"recurrenceType": "PERIODIC"}'::jsonb,
				true, NULL, false, '{}'::jsonb, false;
            RAISE NOTICE '[INFO]: Inserted task_reward.';
        ELSE
			RAISE NOTICE '[INFO]: Task reward exists: %', v_task_reward_code;
        END IF;

        -- Step 7: Cohort linking in cohort.cohort_tenant_task_reward
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
				NOW(), NULL, 'SYSTEM', NULL, 0;
            RAISE NOTICE '[INFO]: Linked cohort with task_reward.';
        ELSE
            RAISE NOTICE '[INFO]: Cohort-task_reward link exists.';
        END IF;

        RAISE NOTICE '✅ Script completed successfully.';

    EXCEPTION
        WHEN OTHERS THEN
            RAISE NOTICE '❌ Error occurred: %', SQLERRM;
            RAISE EXCEPTION 'Transaction rolled back due to error.';
    END;
END $$;
