-- ==================================================================================================
-- 🚀 Script    : Add or Replace Trivia Setup (Soft Delete + Recreate Trivia)
-- 📌 Purpose   : For a given tenant_code and task_external_code, soft delete existing trivia entries
--                and recreate a new trivia with fresh trivia_question_group links.
-- 🧑 Author    : Siva Krishna 
-- 📅 Date      : 2025-10-17
-- 🧾 Jira      : RES-684 (Defect)
-- ⚠️ Inputs    : HAP_TENANT_CODE (replace placeholder below)
-- 📤 Output    : Soft deletes old trivia, inserts new trivia, updates/creates trivia_questions,
--                and re-inserts trivia_question_group links.
-- 🔗 Script URL: NA
-- 📝 Notes     : Always creates new trivia entry for given tenant/task_external_code.
-- ==================================================================================================

DO
$$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- Replace with actual tenant code

    v_input_json JSONB := '{
      "trivia": {
        "triviaTaskExternalCode": "lear_abou_pres_home_deli",
        "ctaTaskExternalCode": "play_now",
        "config": {
        "ux": {
        "questionIcon": {
          "url": "",
          "bgColor": "#111111",
          "fgColor": "#FFFFFF"
        },
        "backgroundUrl": "",
        "wrongAnswerIcon": {
          "url": "",
          "bgColor": "#FF0000",
          "fgColor": "#FFFFFF"
        },
        "correctAnswerIcon": {
          "url": "",
          "bgColor": "#111111",
          "fgColor": "#FFFFFF"
        }
        }
      }
      },
      "triviaQuestions": [
        {
        "en-US": {
        "learning": {
          "header": "Did you know?",
          "description": "Pharmacy Advantage is the name of the pharmacy that fulfills prescription home delivery."
        },
        "answerText": [
          "Pharmacy Advantage",
          "Pharmacy Direct",
          "HAP Pharmacy"
        ],
        "answerType": "SINGLE",
        "layoutType": "BUTTON",
        "validEndTs": "2027-01-01T00:00:00Z",
        "questionText": "What is the name of the pharmacy that fulfills prescription home delivery?",
        "validStartTs": "2025-09-01T00:00:00Z",
        "correctAnswer": [
          0
        ],
        "questionExternalCode": "what_is_the_name_of_the_phar_that_fulf_pres_home_deli"
        }
      },
        {
        "en-US": {
        "learning": {
          "header": "Did you know?",
          "description": "There are 3 ways to sign-up for home delivery. Choose an option - by phone,  online or by printing and mailing a paper copy of the enrollment form  "
        },
        "answerText": [
          "By calling (800) 456-2112 during business hours.",
          "Signing up online using the hyperlink to the enrollment form.",
          "Printing, completing and mailing back a paper copy of the enrollment form.",
          "All of the above"
        ],
        "answerType": "SINGLE",
        "layoutType": "BUTTON",
        "validEndTs": "2027-01-01T00:00:00Z",
        "questionText": "How can someone sign up for home delivery?",
        "validStartTs": "2025-09-01T00:00:00Z",
        "correctAnswer": [
          3
        ],
        "questionExternalCode": "how_can_some_sign_up_for_home_deli"
        }
      },
        {
        "en-US": {
        "learning": {
          "header": "Did you know?",
          "description": "Switching to home delivery is free.  You will only pay your prescription copay."
        },
        "answerText": [
          "True",
          "False"
        ],
        "answerType": "SINGLE",
        "layoutType": "BUTTON",
        "validEndTs": "2027-01-01T00:00:00Z",
        "questionText": "There is no cost for switching to home delivery, you just have to pay your prescription copay.",
        "validStartTs": "2025-09-01T00:00:00Z",
        "correctAnswer": [
          0
        ],
        "questionExternalCode": "ther_is_no_cost_for_swit_to_home_deli_you_just_have_to_pay_your_pres_copay"
        }
      }
      ]
    }'::jsonb;

    v_task_external_code TEXT;
    v_cta_task_external_code TEXT;
    v_task_reward_id BIGINT;
    v_trivia_id BIGINT;
    v_new_trivia_code UUID;

    v_config_json JSONB;
    v_question JSONB;
    v_en_json JSONB;

    v_question_external_code TEXT;
    v_existing_code TEXT;
    v_existing_json JSONB;

    v_question_code TEXT;
    v_trivia_json JSONB;

    v_trivia_question_id BIGINT;
    v_sequence_nbr INT := 0;

    v_valid_start_ts TIMESTAMP;
    v_valid_end_ts TIMESTAMP;

BEGIN
    -- Extract basic values
    v_task_external_code := v_input_json -> 'trivia' ->> 'triviaTaskExternalCode';
    v_cta_task_external_code := v_input_json -> 'trivia' ->> 'ctaTaskExternalCode';
    v_config_json := v_input_json -> 'trivia' -> 'config';

    -- Fetch task_reward_id
    SELECT task_reward_id INTO v_task_reward_id
    FROM task.task_reward
    WHERE task_external_code = v_task_external_code
      AND tenant_code = v_tenant_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_task_reward_id IS NULL THEN
        RAISE EXCEPTION '❌ task_reward_id not found for % (tenant: %)', v_task_external_code, v_tenant_code;
    END IF;

    RAISE NOTICE '✅ Found task_reward_id: %', v_task_reward_id;

    -- Soft delete existing trivia for this task_reward_id
    UPDATE task.trivia
    SET delete_nbr = trivia_id,
        update_ts = now(),
        update_user = 'SYSTEM'
    WHERE task_reward_id = v_task_reward_id
      AND delete_nbr = 0;

    RAISE NOTICE '🧹 Soft deleted existing trivia for task_reward_id: %', v_task_reward_id;

    -- Insert new trivia
    v_new_trivia_code := gen_random_uuid();
    INSERT INTO task.trivia (
        trivia_code, task_reward_id, cta_task_external_code, config_json,
        create_ts, update_ts, create_user, update_user, delete_nbr
    ) VALUES (
        'trv-' || v_new_trivia_code, v_task_reward_id, v_cta_task_external_code, v_config_json::jsonb,
        now(), NULL, 'SYSTEM', NULL, 0
    )
    RETURNING trivia_id INTO v_trivia_id;

    RAISE NOTICE '✅ Inserted new trivia with ID: %', v_trivia_id;

    -- Process trivia questions
    FOR v_question IN SELECT * FROM jsonb_array_elements(v_input_json -> 'triviaQuestions')
    LOOP
        v_en_json := v_question -> 'en-US';
        IF v_en_json IS NULL THEN
            RAISE NOTICE '⚠️ Skipping question without en-US: %', v_question;
            CONTINUE;
        END IF;

        v_question_external_code := lower(v_en_json ->> 'questionExternalCode');
        IF v_question_external_code IS NULL OR trim(v_question_external_code) = '' THEN
            RAISE NOTICE '⚠️ Skipping invalid question_external_code';
            CONTINUE;
        END IF;

        v_trivia_json := jsonb_build_object('en-US', v_en_json);

        -- Check existing question
        SELECT trivia_question_code, trivia_json
        INTO v_existing_code, v_existing_json
        FROM task.trivia_question
        WHERE question_external_code = v_question_external_code
          AND delete_nbr = 0
        LIMIT 1;

        IF (v_en_json ->> 'validStartTs') IS NOT NULL THEN
            v_valid_start_ts := (v_en_json ->> 'validStartTs')::timestamp;
        ELSE
            v_valid_start_ts := NULL;
        END IF;

        IF (v_en_json ->> 'validEndTs') IS NOT NULL THEN
            v_valid_end_ts := (v_en_json ->> 'validEndTs')::timestamp;
        ELSE
            v_valid_end_ts := NULL;
        END IF;

        IF v_existing_code IS NULL THEN
            -- Insert new question
            v_question_code := 'trq-' || gen_random_uuid();
            INSERT INTO task.trivia_question (
                trivia_question_code, question_external_code, trivia_json,
                create_ts, update_ts, create_user, update_user, delete_nbr
            ) VALUES (
                v_question_code, v_question_external_code, v_trivia_json,
                now(), NULL, 'SYSTEM', NULL, 0
            )
            RETURNING trivia_question_id INTO v_trivia_question_id;

            RAISE NOTICE '✅ Inserted new trivia_question: %', v_question_code;
        ELSE
            -- Update existing question JSON only
            UPDATE task.trivia_question
            SET trivia_json = v_trivia_json,
                update_ts = now(),
                update_user = 'SYSTEM'
            WHERE trivia_question_code = v_existing_code
              AND delete_nbr = 0
            RETURNING trivia_question_id INTO v_trivia_question_id;

            RAISE NOTICE '🔁 Updated trivia_question: %', v_existing_code;
        END IF;

        -- Always insert a new link into trivia_question_group
        INSERT INTO task.trivia_question_group (
            trivia_id, trivia_question_id, sequence_nbr,
            valid_start_ts, valid_end_ts,
            create_ts, update_ts, create_user, update_user, delete_nbr
        ) VALUES (
            v_trivia_id, v_trivia_question_id, v_sequence_nbr,
            v_valid_start_ts, v_valid_end_ts,
            now(), NULL, 'SYSTEM', NULL, 0
        );

        RAISE NOTICE '✅ Linked question % to new trivia (sequence %)', v_question_external_code, v_sequence_nbr;

        v_sequence_nbr := v_sequence_nbr + 1;
    END LOOP;

    RAISE NOTICE '🎯 ✅ Completed full trivia recreation for tenant: %', v_tenant_code;

END
$$;
