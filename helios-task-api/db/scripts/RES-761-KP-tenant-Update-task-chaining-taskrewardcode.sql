-- ============================================================================
-- 🚀 Script    : Setup chaining task for trivia 
-- 📌 Purpose   : Purpose of this script is to setup chaining trivia, upon main trivia completion and based on user's dob
--					the admin.script will add daily/weekly tivia to the user.
-- 🧑 Author    : Kumar Sirikonda
-- 📅 Date      : 2025-10-23
-- 🧾 Jira      : RES-761
-- ⚠️ Inputs    : v_tenant_code (TEXT), 
-- 📤 Output    : Success notice.
-- 🔗 Script URL:https://github.com/SunnyRewards/helios-task-api/blob/develop/db/scripts/RES-761-KP-tenant-Update-task-chaining-taskrewardcode.sql
-- ============================================================================
DO
$$
DECLARE
    v_tenant_code           VARCHAR := '<KP_TENANT_CODE>';       -- Input tenant_code
	
    v_task_external_code    VARCHAR := 'play_heal_triv_2026';-- variable task_external_code
    v_script_code           VARCHAR := 'src-20809ec035f24caf936f65e8e354975b'; -- variable -> DOBBasedCohortAssignment
    v_task_reward_code      VARCHAR;
    v_script_id             BIGINT;
    v_tenant_task_reward_script_id BIGINT;
    v_json_node             TEXT;
    v_script_source         TEXT;
BEGIN
    -- STEP 1: Fetch task_reward_code
    SELECT task_reward_code
    INTO v_task_reward_code
    FROM task.task_reward
    WHERE tenant_code = v_tenant_code
      AND task_external_code = v_task_external_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_task_reward_code IS NULL THEN
        RAISE NOTICE '⚠️ Task reward not found for tenant_code: %, task_external_code: %', v_tenant_code, v_task_external_code;
        RETURN;
    END IF;

    RAISE NOTICE '✅ Found task_reward_code: %', v_task_reward_code;

    -- STEP 2: Check or insert into tenant_task_reward_script
    SELECT tenant_task_reward_script_id, script_id
    INTO v_tenant_task_reward_script_id, v_script_id
    FROM admin.tenant_task_reward_script
    WHERE tenant_code = v_tenant_code
      AND task_reward_code = v_task_reward_code
	  AND script_type = 'TASK_COMPLETE_POST'
      AND delete_nbr = 0
    LIMIT 1;

    IF v_tenant_task_reward_script_id IS NULL THEN
        -- Fetch script_id from admin.script
        SELECT script_id
        INTO v_script_id
        FROM admin.script
        WHERE script_code = v_script_code
          AND delete_nbr = 0
        LIMIT 1;

        IF v_script_id IS NULL THEN
            RAISE NOTICE '⚠️ Script not found for script_code: %', v_script_code;
            RETURN;
        END IF;

        -- Insert new tenant_task_reward_script record
        INSERT INTO admin.tenant_task_reward_script (
            tenant_task_reward_script_code, tenant_code, task_reward_code,
            script_type, script_id, create_ts, create_user, delete_nbr
        ) VALUES (
            'trs-' || gen_random_uuid(),
            v_tenant_code,
            v_task_reward_code,
            'TASK_COMPLETE_POST',
            v_script_id,
            NOW(),
            'SYSTEM',
            0
        )
        RETURNING tenant_task_reward_script_id INTO v_tenant_task_reward_script_id;

        RAISE NOTICE '✅ Inserted new tenant_task_reward_script with ID: %', v_tenant_task_reward_script_id;
    ELSE
        RAISE NOTICE 'ℹ️ Found existing tenant_task_reward_script with ID: %', v_tenant_task_reward_script_id;
    END IF;

    -- STEP 3: Build JSON node as text
    v_json_node := format(
        '{
            "odd_dob_cohort": "adult18up+odd_dob",
            "even_dob_cohort": "adult18up+even_dob",
            "task_reward_code": "%s"
        }', v_task_reward_code
    );

    -- STEP 4: Get current script_source
    SELECT script_source
    INTO v_script_source
    FROM admin.script
    WHERE script_id = v_script_id
      AND delete_nbr = 0;

    IF v_script_source IS NULL THEN
        RAISE NOTICE '⚠️ No script_source found for script_id: %', v_script_id;
        RETURN;
    END IF;

    -- STEP 5: Check if task_reward_code already exists
    IF POSITION(v_task_reward_code IN v_script_source) > 0 THEN
        RAISE NOTICE 'ℹ️ Task reward code % already exists in script_source. Skipping update.', v_task_reward_code;
        RETURN;
    END IF;

    -- STEP 6: Append JSON node to taskRewardCohorts array
    UPDATE admin.script
    SET script_source = regexp_replace(
        script_source,
        '(\s*const\s+taskRewardCohorts\s*=\s*\[[^\]]*)',
        E'\\1,\n\t' || v_json_node,
        'g'
    )
    WHERE script_id = v_script_id
      AND delete_nbr = 0;

    RAISE NOTICE '✅ Appended new JSON node to script_id: %', v_script_id;

END
$$ LANGUAGE plpgsql;
