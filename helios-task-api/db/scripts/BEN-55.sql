DO $$
DECLARE
    -- ====== Inputs ======
    
	
	--v_tenant_code        TEXT := 'ten-b4e920d3f6f74496ab533d1a9a8ef9e4';
	v_tenant_code        TEXT := '<HAP-TENANT-CODE>';
	
	v_cohort_name  TEXT := 'everyone';
    v_task_name TEXT := 'Onboarding Survey';
    -- english
    v_task_detail_name_english        TEXT := 'Onboarding Survey';
    v_task_detail_description_english TEXT := 'Onboarding Survey is trivia task to show in Onboarding flow for HAP';
    -- spanish
    v_task_detail_name_spanish        TEXT := 'Onboarding Survey';
    v_task_detail_description_spanish TEXT := 'Onboarding Survey is trivia task to show in Onboarding flow for HAP';

    v_task_external_code TEXT := 'Onboard_survey';
    v_task_type_code     TEXT := 'tty-5c44328dce5a4b60ab79ab13e9253f27';

    -- reward type (as used in your later block)
    v_task_reward_type_code TEXT := 'rtc-a5a943d3fc2a4506ab12218204d60805';
    v_reward_amount         NUMERIC := 0;
    v_reward_type           TEXT := 'MONETARY_DOLLARS';

    -- Trivia config JSON
    v_trivia_config_json JSONB := '{
      "ux": {
        "questionIcon": { "url": "", "bgColor": "#111111", "fgColor": "#FFFFFF" },
        "backgroundUrl": "",
        "wrongAnswerIcon": { "url": "", "bgColor": "#FF0000", "fgColor": "#FFFFFF" },
        "correctAnswerIcon": { "url": "", "bgColor": "#111111", "fgColor": "#FFFFFF" }
      }
    }'::jsonb;

    -- ====== Internals / generated ======
    v_cohort_id   BIGINT;
    v_cohort_code TEXT;

    v_task_type_id BIGINT;
    v_task_id      BIGINT;
    v_task_code    TEXT;
    v_task_detail_id BIGINT;

    v_task_reward_type_id BIGINT;
    v_task_reward_id      BIGINT;
    v_task_reward_code    TEXT;

    -- Trivia + Question vars (insert-only requirement)
    v_trivia_id BIGINT;
    v_trivia_code TEXT;

    v_trivia_question_id BIGINT;
    v_trivia_question_group_id BIGINT;

    v_en_json JSONB;
    v_question_external_code TEXT := 'rate_your_health_plan_general';

    v_sequence_nbr INT;
    v_valid_start_ts TIMESTAMP := NOW();
    v_valid_end_ts   TIMESTAMP := '2100-01-01 23:59:59';

BEGIN
  BEGIN
    ---------------------------------------------------------------------------
    -- 0) Everyone Cohort must exist
    ---------------------------------------------------------------------------
    SELECT cohort_id, cohort_code
      INTO v_cohort_id, v_cohort_code
    FROM cohort.cohort
    WHERE cohort_name = v_cohort_name
      AND delete_nbr = 0
    LIMIT 1;

    IF v_cohort_id IS NULL THEN
      RAISE EXCEPTION 'Cohort "%" not found', v_cohort_name;
    END IF;
    RAISE NOTICE 'Cohort: id=%, code=%', v_cohort_id, v_cohort_code;

    ---------------------------------------------------------------------------
    -- 1) Task type must exist
    ---------------------------------------------------------------------------
    SELECT task_type_id
      INTO v_task_type_id
    FROM task.task_type
    WHERE task_type_code = v_task_type_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_task_type_id IS NULL THEN
      RAISE EXCEPTION 'task_type_code "%" not found', v_task_type_code;
    END IF;
	
	---------------------------------------------------------------------------
    -- 2) Reward type lookup
    ---------------------------------------------------------------------------
    SELECT reward_type_id
      INTO v_task_reward_type_id
    FROM task.reward_type
    WHERE reward_type_code = v_task_reward_type_code and delete_nbr = 0 
    LIMIT 1;

    IF v_task_reward_type_id IS NULL THEN
      RAISE EXCEPTION 'reward_type_code "%" not found', v_task_reward_type_code;
    END IF;

    ---------------------------------------------------------------------------
    -- 3) Create or reuse task (by exact name)
    ---------------------------------------------------------------------------
    SELECT t.task_id, t.task_code
      INTO v_task_id, v_task_code
    FROM task.task t
    WHERE t.task_name = v_task_name
      AND t.delete_nbr = 0
    LIMIT 1;

    IF v_task_id IS NULL THEN
      v_task_code := 'tsk-' || REPLACE(gen_random_uuid()::text, '-', '');
      INSERT INTO task.task (
          task_type_id, task_code, task_name,
          create_ts, update_ts, create_user, update_user,
          delete_nbr, self_report, confirm_report, task_category_id, is_subtask
      )
      VALUES (
          v_task_type_id, v_task_code, v_task_name,
          NOW(), NULL, 'SYSTEM', NULL,
          0, true, false, NULL, false
      )
      RETURNING task_id INTO v_task_id;

      RAISE NOTICE 'Inserted task: task_id=%, task_code=%', v_task_id, v_task_code;
    ELSE
      RAISE NOTICE 'Task exists: task_id=%, task_code=%', v_task_id, v_task_code;
    END IF;

    ---------------------------------------------------------------------------
    -- 4) Task details (tenant-scoped), EN
    ---------------------------------------------------------------------------
    -- EN
    INSERT INTO task.task_detail (
        task_id, language_code, task_header, task_description, terms_of_service_id,
        create_ts, update_ts, create_user, update_user, delete_nbr,
        task_cta_button_text, tenant_code
    )
    SELECT v_task_id, 'en-US', v_task_detail_name_english, v_task_detail_description_english, 1,
           NOW(), NULL, 'SYSTEM', NULL, 0, 'Enroll Now', v_tenant_code
    WHERE NOT EXISTS (
      SELECT 1 FROM task.task_detail
      WHERE task_id = v_task_id AND tenant_code = v_tenant_code
        AND language_code = 'en-US' AND delete_nbr = 0
    );
    IF FOUND THEN
      RAISE NOTICE 'Inserted EN task_detail for task_id=%', v_task_id;
    END IF;



    ---------------------------------------------------------------------------
    -- 5) Create or reuse task_reward (tenant-scoped)
    ---------------------------------------------------------------------------
    v_task_reward_code := 'trw-' || REPLACE(gen_random_uuid()::text, '-', '');

    INSERT INTO task.task_reward (
        task_id, reward_type_id, tenant_code, task_reward_code, reward, min_task_duration,
        max_task_duration, expiry, priority, create_ts, update_ts, create_user,
        update_user, delete_nbr, task_action_url, task_external_code, valid_start_ts,
        is_recurring, recurrence_definition_json, self_report, task_completion_criteria_json,
        confirm_report, task_reward_config_json, is_collection
    )
    SELECT v_task_id, v_task_reward_type_id, v_tenant_code, v_task_reward_code,
           jsonb_build_object('rewardType', v_reward_type, 'rewardAmount', v_reward_amount, 'membershipType', NULL),
           0, 0, '2100-01-01 00:00:00', -10, NOW(), NULL, 'SYSTEM',
           NULL, 0, NULL, v_task_external_code,
           v_valid_start_ts, false,
           '{}'::jsonb,
           true, NULL, false, jsonb_build_object('IsOnBoardingSurvey', true), false
    WHERE NOT EXISTS (
      SELECT 1 FROM task.task_reward
      WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0
    )
    RETURNING task_reward_id, task_reward_code
      INTO v_task_reward_id, v_task_reward_code;

    IF FOUND THEN
      RAISE NOTICE 'Inserted task_reward: task_reward_id=%, task_reward_code=%', v_task_reward_id, v_task_reward_code;
    ELSE
      SELECT task_reward_id, task_reward_code
        INTO v_task_reward_id, v_task_reward_code
      FROM task.task_reward
      WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0
      LIMIT 1;

      RAISE NOTICE 'Task reward exists: task_reward_id=%, task_reward_code=%', v_task_reward_id, v_task_reward_code;
    END IF;

    ---------------------------------------------------------------------------
    -- 6) Link cohort ↔ tenant ↔ task_reward (insert if missing)
    ---------------------------------------------------------------------------
    INSERT INTO cohort.cohort_tenant_task_reward (
        cohort_id, tenant_code, task_reward_code, recommended, priority,
        create_ts, update_ts, create_user, update_user, delete_nbr
    )
    SELECT v_cohort_id, v_tenant_code, v_task_reward_code, true, -10,
           NOW(), NULL, 'SYSTEM', NULL, 0
    WHERE NOT EXISTS (
      SELECT 1 FROM cohort.cohort_tenant_task_reward
      WHERE cohort_id = v_cohort_id
        AND tenant_code = v_tenant_code
        AND task_reward_code = v_task_reward_code
        AND delete_nbr = 0
    );
    IF FOUND THEN
      RAISE NOTICE 'Linked cohort to task_reward.';
    ELSE
      RAISE NOTICE 'Cohort↔task_reward link already exists.';
    END IF;

    ---------------------------------------------------------------------------
    -- 7) INSERT-ONLY: task.trivia for this task_reward (no updates)
    ---------------------------------------------------------------------------
    SELECT t.trivia_id, t.trivia_code
      INTO v_trivia_id, v_trivia_code
    FROM task.trivia t
    WHERE t.task_reward_id = v_task_reward_id
      AND t.delete_nbr = 0
    LIMIT 1;

    IF v_trivia_id IS NULL THEN
      v_trivia_code := 'trv-' || REPLACE(gen_random_uuid()::text, '-', '');
      INSERT INTO task.trivia (
          trivia_code, task_reward_id, cta_task_external_code, config_json,
          create_ts, update_ts, create_user, update_user, delete_nbr
      )
      VALUES (
          v_trivia_code,
          v_task_reward_id,
          null,      
          v_trivia_config_json,
          NOW(), NULL, 'SYSTEM', NULL, 0
      )
      RETURNING trivia_id INTO v_trivia_id;

      RAISE NOTICE 'Inserted task.trivia: trivia_id=%, trivia_code=%', v_trivia_id, v_trivia_code;
    ELSE
      RAISE NOTICE 'task.trivia exists: trivia_id=% (no update per requirement)', v_trivia_id;
    END IF;

    ---------------------------------------------------------------------------
    -- 8) INSERT-ONLY: the "rate your health plan" question (no updates)
    ---------------------------------------------------------------------------
    v_en_json := jsonb_build_object(
        'learning', jsonb_build_object(
            'header', '',
            'description', ''
        ),
        'answerText',  jsonb_build_array('Excellent','Very Good','Good','Fair','Poor'),
        'answerType',  'SINGLE',
        'layoutType',  'BUTTON',
        'questionText','In general, how would you rate your health plan?',
        'correctAnswer','[0,1,2,3,4]'::jsonb,
        'isScored',    false,
        'questionExternalCode', v_question_external_code,
        'validStartTs', to_char(v_valid_start_ts, 'YYYY-MM-DD"T"HH24:MI:SS"Z"'),
        'validEndTs',   to_char(v_valid_end_ts,   'YYYY-MM-DD"T"HH24:MI:SS"Z"')
    );

    SELECT trivia_question_id
      INTO v_trivia_question_id
    FROM task.trivia_question
    WHERE question_external_code = v_question_external_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_trivia_question_id IS NULL THEN
        INSERT INTO task.trivia_question (
            trivia_question_code, question_external_code, trivia_json,
            create_ts, update_ts, create_user, update_user, delete_nbr
        )
        VALUES (
            'trq-' || REPLACE(gen_random_uuid()::text,'-',''),
            v_question_external_code,
            jsonb_build_object('en-US', v_en_json),
            NOW(), NULL, 'SYSTEM', NULL, 0
        )
        RETURNING trivia_question_id INTO v_trivia_question_id;

        RAISE NOTICE 'Inserted trivia_question: id=%', v_trivia_question_id;
    ELSE
        RAISE NOTICE 'trivia_question exists: id=% (no update per requirement)', v_trivia_question_id;
    END IF;

    ---------------------------------------------------------------------------
    -- 9) INSERT-ONLY: link in task.trivia_question_group (no updates)
    ---------------------------------------------------------------------------
    -- Next sequence within this trivia
    SELECT COALESCE(MAX(sequence_nbr) + 1, 1)
      INTO v_sequence_nbr
    FROM task.trivia_question_group
    WHERE trivia_id = v_trivia_id
      AND delete_nbr = 0;

    IF NOT EXISTS (
        SELECT 1
        FROM task.trivia_question_group
        WHERE trivia_id = v_trivia_id
          AND trivia_question_id = v_trivia_question_id
          AND delete_nbr = 0
    ) THEN
        INSERT INTO task.trivia_question_group (
            trivia_id, trivia_question_id, sequence_nbr,
            valid_start_ts, valid_end_ts,
            create_ts, update_ts, create_user, update_user, delete_nbr
        )
        VALUES (
            v_trivia_id, v_trivia_question_id, v_sequence_nbr,
            v_valid_start_ts, v_valid_end_ts,
            NOW(), NULL, 'SYSTEM', NULL, 0
        )
        RETURNING trivia_question_group_id INTO v_trivia_question_group_id;

        RAISE NOTICE 'Linked question to trivia: group_id=% seq=%', v_trivia_question_group_id, v_sequence_nbr;
    ELSE
        RAISE NOTICE 'trivia_question_group link already exists (no update per requirement).';
    END IF;

    RAISE NOTICE 'Script completed successfully (insert-only for trivia components).';

  EXCEPTION WHEN OTHERS THEN
    RAISE NOTICE 'Error occurred: %', SQLERRM;
    RAISE EXCEPTION 'Transaction rolled back due to error.';
  END;
END $$ LANGUAGE plpgsql;
