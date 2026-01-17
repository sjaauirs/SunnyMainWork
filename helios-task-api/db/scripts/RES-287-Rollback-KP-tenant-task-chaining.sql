DO
$$
DECLARE
    v_tenant_code        VARCHAR := '<KP-TENANT-CODE>';       -- Input tenant_code
    v_task_external_code VARCHAR := 'play_heal_triv_2026';    -- Input task_external_code
    v_script_code        VARCHAR := 'src-20809ec035f24caf936f65e8e354975b'; -- Input script_code

    v_task_reward_code   VARCHAR;
    v_script_id          BIGINT;
    v_script_source      TEXT;
	v_rows_updated       INT;
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
        RAISE EXCEPTION '❌ Task reward not found for tenant_code: %, task_external_code: %', v_tenant_code, v_task_external_code;
    END IF;

    -- STEP 2: Get script_id
    SELECT script_id
    INTO v_script_id
    FROM admin.script
    WHERE script_code = v_script_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_script_id IS NULL THEN
        RAISE EXCEPTION '❌ Script not found for script_code: %', v_script_code;
    END IF;

    -- STEP 3: Get script_source
    SELECT script_source
    INTO v_script_source
    FROM admin.script
    WHERE script_id = v_script_id
      AND delete_nbr = 0;

    IF v_script_source IS NULL THEN
        RAISE EXCEPTION '❌ No script_source found for script_id: %', v_script_id;
    END IF;

    -- STEP 4: Remove the JSON node AND clean up extra commas
    UPDATE admin.script
    SET script_source = regexp_replace(
        script_source,
        '(\s*,?\s*\{\s*"odd_dob_cohort":\s*"adult18up\+odd_dob",\s*"even_dob_cohort":\s*"adult18up\+even_dob",\s*"task_reward_code":\s*"' || v_task_reward_code || '"\s*\})',
        '',
        'g'
    )
    WHERE script_id = v_script_id
      AND delete_nbr = 0;

    -- Extra cleanup: Remove a leading comma if left at array start
    UPDATE admin.script
    SET script_source = regexp_replace(
        script_source,
        '(\[\s*),\s*',
        '\1',
        'g'
    )
    WHERE script_id = v_script_id
      AND delete_nbr = 0;

    RAISE NOTICE '✅ Cleaned up JSON node for task_reward_code: % in script_id: %', v_task_reward_code, v_script_id;
	UPDATE admin.tenant_task_reward_script
    SET delete_nbr = tenant_task_reward_script_id,
        update_ts = NOW(),
        update_user = 'SYSTEM'
    WHERE tenant_code = v_tenant_code
      AND task_reward_code = v_task_reward_code
      AND script_type = 'TASK_COMPLETE_POST'
      AND delete_nbr = 0;

    GET DIAGNOSTICS v_rows_updated = ROW_COUNT;

    IF v_rows_updated = 0 THEN
        RAISE NOTICE 'ℹ️ No tenant_task_reward_script entry found to soft delete for tenant_code: %, task_reward_code: %',
            v_tenant_code, v_task_reward_code;
    ELSE
        RAISE NOTICE '✅ Soft deleted % tenant_task_reward_script entry for tenant_code: %, task_reward_code: %',
            v_rows_updated, v_tenant_code, v_task_reward_code;
    END IF;

END
$$ LANGUAGE plpgsql;