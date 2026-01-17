-- ============================================================================
-- 🚀 Script    : KP Onboarding Survey – Task, Reward & Questionnaire Setup
-- 📌 Purpose   : 
--   - Creates or reuses the “Onboarding Survey – Life Stage Best Fit” task for Kaiser Permanente.
--   - Ensures required task details (English & Spanish), task reward, and cohort-task linkage.
--   - Inserts or validates the onboarding questionnaire and associated “life_stage_selection” question.
--   - Updates questionnaire JSON to include both 'en-US' and 'es' translations if already present.
-- 🧠 Behavior  :
--   - Idempotent: Safely re-runnable; skips inserts if records already exist.
--   - Performs insert-only operations for questionnaire and question creation.
--   - Logs all major actions with visual status indicators.
-- 🧩 Modules Touched :
--      • cohort.cohort
--      • cohort.cohort_tenant_task_reward
--      • task.task
--      • task.task_detail
--      • task.task_reward
--      • task.reward_type
--      • task.task_type
--      • task.questionnaire
--      • task.questionnaire_question
--      • task.questionnaire_question_group
-- 👨‍💻 Author   : Rakesh Pernati
-- 📅 Date      : 2025-10-12
-- 🧾 Jira      : BEN-847
-- ⚙️ Inputs    :
--      v_tenant_code        → Tenant identifier (e.g., 'ten-153bd6c47ebe4673a75c71faa22b9eb6')
-- 📤 Output    :
--      - Task, reward, and cohort linkage established or verified
--      - Questionnaire and question created or updated
--      - All actions logged to console with success/warning markers
-- 🛠️ Notes     :
--      - Safe to re-run (idempotent)
--      - Does not delete or overwrite unrelated data
--      - Updates only questionnaire JSON (replacing with proper 'en-US'/'es' keys)
-- ============================================================================


DO $$
DECLARE
    -- ====== Inputs ======
    v_tenant_code        TEXT := '<KP-TENANT-CODE>';
    v_cohort_name        TEXT := 'everyone';

    v_task_name          TEXT := 'Onboarding Survey life stage best fit';
    v_task_detail_name_english        TEXT := 'life stage best fit';
    v_task_detail_description_english TEXT := 'Onboarding Survey for Kaiser Permanente';
    v_task_detail_name_spanish        TEXT := 'life stage best fit';
    v_task_detail_description_spanish TEXT := 'Onboarding Survey for Kaiser Permanente';

    v_task_external_code TEXT := 'KP_Onboarding_Survey_life_stage_best_fit';
    v_task_type_code     TEXT := 'tty-86398dc3a77d4a3db7922e57b5b6d73c'; -- survey task type

    v_task_reward_type_code TEXT := 'rtc-a5a943d3fc2a4506ab12218204d60805';
    v_reward_amount         NUMERIC := 5;
    v_reward_type           TEXT := 'MONETARY_DOLLARS';

    v_config_json JSONB := '{
      "ux": {
        "questionIcon":     { "url": "", "bgColor": "#111111", "fgColor": "#FFFFFF" },
        "backgroundUrl": "",
        "wrongAnswerIcon":  { "url": "", "bgColor": "#FF0000", "fgColor": "#FFFFFF" },
        "correctAnswerIcon":{ "url": "", "bgColor": "#111111", "fgColor": "#FFFFFF" }
      }
    }'::jsonb;

    -- Question payload inputs
    v_question_external_code TEXT := 'life_stage_selection';
    v_valid_start_ts TIMESTAMPTZ := '2025-07-01 00:00:00+00';
    v_valid_end_ts   TIMESTAMPTZ := '2026-08-31 23:59:59+00';

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

    v_questionnaire_id    BIGINT;
    v_questionnaire_code  TEXT;

    v_questionnaire_question_id        BIGINT;
    v_questionnaire_question_group_id  BIGINT;

    v_en_json JSONB;
    v_es_json JSONB;
    v_sequence_nbr INT;
BEGIN
    RAISE NOTICE '🟦 [START] Executing KP Onboarding Survey Setup...';

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
        RAISE EXCEPTION '❌ Cohort "%" not found', v_cohort_name;
    END IF;
    RAISE NOTICE '✅ Cohort found: id=%, code=%', v_cohort_id, v_cohort_code;

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
        RAISE EXCEPTION '❌ task_type_code "%" not found', v_task_type_code;
    END IF;
    RAISE NOTICE '✅ Task type verified: id=%', v_task_type_id;

    ---------------------------------------------------------------------------
    -- 2) Reward type lookup
    ---------------------------------------------------------------------------
    SELECT reward_type_id
      INTO v_task_reward_type_id
    FROM task.reward_type
    WHERE reward_type_code = v_task_reward_type_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_task_reward_type_id IS NULL THEN
        RAISE EXCEPTION '❌ reward_type_code "%" not found', v_task_reward_type_code;
    END IF;
    RAISE NOTICE '✅ Reward type verified: id=%', v_task_reward_type_id;

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
      v_task_code := 'tsk-' || replace(gen_random_uuid()::text, '-','');
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

      RAISE NOTICE '🟢 Inserted task: task_id=%, task_code=%', v_task_id, v_task_code;
    ELSE
      RAISE NOTICE '⚠️ Task exists: task_id=%, task_code=%', v_task_id, v_task_code;
    END IF;

    ---------------------------------------------------------------------------
    -- 4) Task details (tenant-scoped), EN + ES (insert if missing)
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
      RAISE NOTICE '🟢 Inserted EN task_detail for task_id=%', v_task_id;
    ELSE
      RAISE NOTICE '⚠️ EN task_detail already exists for task_id=% (no-op)', v_task_id;
    END IF;

    -- ES (Spanish)
    INSERT INTO task.task_detail (
        task_id, language_code, task_header, task_description, terms_of_service_id,
        create_ts, update_ts, create_user, update_user, delete_nbr,
        task_cta_button_text, tenant_code
    )
    SELECT v_task_id, 'es', v_task_detail_name_spanish, v_task_detail_description_spanish, 1,
           NOW(), NULL, 'SYSTEM', NULL, 0, 'Inscríbete ahora', v_tenant_code
    WHERE NOT EXISTS (
      SELECT 1 FROM task.task_detail
      WHERE task_id = v_task_id AND tenant_code = v_tenant_code
        AND language_code = 'es' AND delete_nbr = 0
    );
    IF FOUND THEN
      RAISE NOTICE '🟢 Inserted ES task_detail for task_id=%', v_task_id;
    ELSE
      RAISE NOTICE '⚠️ ES task_detail already exists for task_id=% (no-op)', v_task_id;
    END IF;

    ---------------------------------------------------------------------------
    -- 5) Create or reuse task_reward (tenant-scoped)
    ---------------------------------------------------------------------------
    v_task_reward_code := 'trw-' || replace(gen_random_uuid()::text, '-','');

    INSERT INTO task.task_reward (
        task_id, reward_type_id, tenant_code, task_reward_code, reward, min_task_duration,
        max_task_duration, expiry, priority, create_ts, update_ts, create_user,
        update_user, delete_nbr, task_action_url, task_external_code, valid_start_ts,
        is_recurring, recurrence_definition_json, self_report, task_completion_criteria_json,
        confirm_report, task_reward_config_json, is_collection
    )
    SELECT v_task_id, v_task_reward_type_id, v_tenant_code, v_task_reward_code,
           jsonb_build_object('rewardType', v_reward_type, 'rewardAmount', v_reward_amount, 'membershipType', NULL),
           0, 0, ('2100-01-01 00:00:00'::timestamp), 100, NOW(), NULL, 'SYSTEM',
           NULL, 0, NULL, v_task_external_code,
           NOW(), false,
           '{}'::jsonb,
           true, NULL, false, jsonb_build_object('IsOnBoardingSurvey', true), false
    WHERE NOT EXISTS (
      SELECT 1 FROM task.task_reward
      WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0
    )
    RETURNING task_reward_id, task_reward_code
      INTO v_task_reward_id, v_task_reward_code;

    IF NOT FOUND THEN
      SELECT task_reward_id, task_reward_code
        INTO v_task_reward_id, v_task_reward_code
      FROM task.task_reward
      WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0
      LIMIT 1;

      RAISE NOTICE '⚠️ Task reward exists: task_reward_id=%, task_reward_code=%', v_task_reward_id, v_task_reward_code;
    ELSE
      RAISE NOTICE '🟢 Inserted task_reward: task_reward_id=%, task_reward_code=%', v_task_reward_id, v_task_reward_code;
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
      RAISE NOTICE '🟢 Linked cohort to task_reward (inserted).';
    ELSE
      RAISE NOTICE '⚠️ Cohort↔task_reward link already exists (no-op).';
    END IF;

    ---------------------------------------------------------------------------
    -- 7) INSERT-ONLY: task.questionnaire (no updates)
    ---------------------------------------------------------------------------
    SELECT q.questionnaire_id, q.questionnaire_code
      INTO v_questionnaire_id, v_questionnaire_code
    FROM task.questionnaire q
    WHERE q.task_reward_id = v_task_reward_id
      AND q.delete_nbr = 0
    LIMIT 1;

    IF v_questionnaire_id IS NULL THEN
      v_questionnaire_code := 'qsr-' || replace(gen_random_uuid()::text, '-','');
      INSERT INTO task.questionnaire (
          questionnaire_code, task_reward_id, cta_task_external_code, config_json,
          create_ts, update_ts, create_user, update_user, delete_nbr
      )
      VALUES (
          v_questionnaire_code,
          v_task_reward_id,
          NULL,
          v_config_json,
          NOW(), NULL, 'SYSTEM', NULL, 0
      )
      RETURNING questionnaire_id INTO v_questionnaire_id;

      RAISE NOTICE '🟢 Inserted task.questionnaire: questionnaire_id=%, questionnaire_code=%', v_questionnaire_id, v_questionnaire_code;
    ELSE
      RAISE NOTICE '⚠️ task.questionnaire exists: questionnaire_id=% (no update per requirement)', v_questionnaire_id;
    END IF;

    ---------------------------------------------------------------------------
    -- 8) INSERT-ONLY: the "life_stage_selection" question (no updates)
    ---------------------------------------------------------------------------
    -- Build EN JSON
    v_en_json := jsonb_build_object(
        'answerText', jsonb_build_array(
            'Mental health',
            'Stress management',
            'Better sleep',
            'Women''s health',
            'Senior health',
            'Fitness and exercise',
            'Healthy eating',
            'Parenting support',
            'None of these'
        ),
        'answerType', 'MULTI',
        'layoutType', 'CHECKBOX',
        'maxSelect', 3,
        'questionText', 'Choose up to 3, and we’ll send you helpful resources to support your health and well being.',
        'instructionText', 'What health topics interest you the most?',
        'validStartTs', to_char(v_valid_start_ts AT TIME ZONE 'UTC', 'YYYY-MM-DD"T"HH24:MI:SS"Z"'),
        'validEndTs',   to_char(v_valid_end_ts   AT TIME ZONE 'UTC', 'YYYY-MM-DD"T"HH24:MI:SS"Z"'),
        'correctAnswer',  jsonb_build_array(0,1,2,3,4,5,6,7,8),
        'questionExternalCode', v_question_external_code,
		'mandatory',true
    );

    -- Build ES JSON
    v_es_json := jsonb_build_object(
        'answerText', jsonb_build_array(
            'Salud mental',
            'Manejo del estrés',
            'Mejor sueño',
            'Salud de la mujer',
            'Salud de las personas mayores',
            'Estado físico y ejercicio',
            'Alimentación saludable',
            'Apoyo para la crianza',
            'Ninguno de estos'
        ),
        'answerType', 'MULTI',
        'layoutType', 'CHECKBOX',
        'maxSelect', 3,
        'questionText', 'Seleccione hasta tres temas que mejor describan su situación actual.',
        'instructionText', '¿Qué temas de salud le interesan más?',
        'validStartTs', to_char(v_valid_start_ts AT TIME ZONE 'UTC', 'YYYY-MM-DD"T"HH24:MI:SS"Z"'),
        'validEndTs',   to_char(v_valid_end_ts   AT TIME ZONE 'UTC', 'YYYY-MM-DD"T"HH24:MI:SS"Z"'),
        'correctAnswer',  jsonb_build_array(0,1,2,3,4,5,6,7,8),
        'questionExternalCode', v_question_external_code,
		'mandatory',true
    );

    -- Check if question exists (by external code)
    SELECT questionnaire_question_id
      INTO v_questionnaire_question_id
    FROM task.questionnaire_question
    WHERE question_external_code = v_question_external_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_questionnaire_question_id IS NULL THEN
        INSERT INTO task.questionnaire_question (
            questionnaire_question_code,
            question_external_code,
            questionnaire_json,
            create_ts, update_ts, create_user, update_user, delete_nbr
        )
        VALUES (
            'qsr-' || replace(gen_random_uuid()::text,'-',''),
            v_question_external_code,
            jsonb_build_object('en-US', v_en_json, 'es', v_es_json),
            NOW(), NULL, 'SYSTEM', NULL, 0
        )
        RETURNING questionnaire_question_id INTO v_questionnaire_question_id;

        RAISE NOTICE '🟢 Inserted questionnaire_question: id=%', v_questionnaire_question_id;
    ELSE
        RAISE NOTICE '🟡 questionnaire_question exists: id=% — updating JSON to ensure en-US/es keys', v_questionnaire_question_id;

        -- Before updating, ensure JSON keys are correctly set to en-US and es
        UPDATE task.questionnaire_question
        SET questionnaire_json = jsonb_build_object(
                'en-US', v_en_json,
                'es', v_es_json
            ),
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE questionnaire_question_id = v_questionnaire_question_id;

        RAISE NOTICE '✅ Updated questionnaire_question.id=% questionnaire_json (replaced with en-US / es structure)', v_questionnaire_question_id;
    END IF;

    ---------------------------------------------------------------------------
    -- 9) INSERT-ONLY: link question to questionnaire (no updates)
    ---------------------------------------------------------------------------
    SELECT COALESCE(MAX(sequence_nbr), 0) + 1
      INTO v_sequence_nbr
    FROM task.questionnaire_question_group
    WHERE questionnaire_id = v_questionnaire_id
      AND delete_nbr = 0;

    IF NOT EXISTS (
        SELECT 1
        FROM task.questionnaire_question_group
        WHERE questionnaire_id = v_questionnaire_id
          AND questionnaire_question_id = v_questionnaire_question_id
          AND delete_nbr = 0
    ) THEN
        INSERT INTO task.questionnaire_question_group (
            questionnaire_id, questionnaire_question_id, sequence_nbr,
            valid_start_ts, valid_end_ts,
            create_ts, update_ts, create_user, update_user, delete_nbr
        )
        VALUES (
            v_questionnaire_id, v_questionnaire_question_id, v_sequence_nbr,
            (NOW() AT TIME ZONE 'UTC'), ('2100-01-01 23:59:59'::timestamp),
            NOW(), NULL, 'SYSTEM', NULL, 0
        )
        RETURNING questionnaire_question_group_id
          INTO v_questionnaire_question_group_id;

        RAISE NOTICE '🟢 Linked question to questionnaire: group_id=% seq=%', v_questionnaire_question_group_id, v_sequence_nbr;
    ELSE
        RAISE NOTICE '⚠️ questionnaire_question_group link already exists (no-op).';
    END IF;

    ---------------------------------------------------------------------------
    -- Summary and completion
    ---------------------------------------------------------------------------
    RAISE NOTICE '------------------------------------------------------------';
    RAISE NOTICE '🎯 SUMMARY';
    RAISE NOTICE '   Tenant Code       : %', v_tenant_code;
    RAISE NOTICE '   Cohort Name       : %', v_cohort_name;
    RAISE NOTICE '   Task ID           : %', v_task_id;
    RAISE NOTICE '   Task Code         : %', v_task_code;
    RAISE NOTICE '   Reward ID         : %', v_task_reward_id;
    RAISE NOTICE '   Reward Code       : %', v_task_reward_code;
    RAISE NOTICE '   Questionnaire ID  : %', v_questionnaire_id;
    RAISE NOTICE '   Question ID       : %', v_questionnaire_question_id;
    RAISE NOTICE '   Question Group ID : %', v_questionnaire_question_group_id;
    RAISE NOTICE '------------------------------------------------------------';
    RAISE NOTICE '✅ [SUCCESS] KP Onboarding Survey script completed successfully!';
EXCEPTION
    WHEN OTHERS THEN
        -- Log the error with message and SQLSTATE
        RAISE WARNING '❌ Script failed: % | SQLSTATE=%', SQLERRM, SQLSTATE;
        -- Optionally you can RAISE the error again to propagate, or swallow depending on your policy.
        RAISE;
END $$;

