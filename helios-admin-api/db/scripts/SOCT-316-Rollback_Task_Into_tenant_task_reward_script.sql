	
	-- ============================================================================
-- 🔁 Rollback Script: Remove inserted rows from admin.tenant_task_reward_script
-- 📌 Purpose:
--     Deletes records from tenant_task_reward_script for the given script_code,
--     tenant_code, script_type, and task_reward_codes that were inserted.
-- 🧑 Author  : Kumar Sirikonda
-- 📅 Date    : 2025-05-27
-- 🧾 Jira    : SOCT-316
-- ⚠️  Inputs:
--     - tenant_code
--     - comma-separated task_reward_code list
--     - script_code
--     - script_type
-- ✅ Behavior:
--     - Deletes only active rows (delete_nbr = 0)
-- ============================================================================


BEGIN
    -- Step 1: Get script_id from script_code
    SELECT script_id INTO v_script_id
    FROM admin.script
    WHERE script_code = v_script_code AND delete_nbr = 0
    LIMIT 1;

    IF v_script_id IS NULL THEN
        RAISE NOTICE '⚠️ Script not found. Nothing to rollback.';
        RETURN;
    END IF;

    -- Step 2: Loop through each task_reward_code
    FOR v_task_reward_code IN SELECT unnest(string_to_array(v_task_reward_codes_list, ','))
    LOOP
        DELETE FROM admin.tenant_task_reward_script
        WHERE script_id = v_script_id
          AND tenant_code = v_tenant_code
          AND script_type = v_script_type
          AND task_reward_code = v_task_reward_code
          AND delete_nbr = 0;

        IF FOUND THEN
            RAISE NOTICE '✅ Rolled back entry for task_reward_code: %', v_task_reward_code;
        ELSE
            RAISE NOTICE '⚠️ No active entry found for task_reward_code: %, skipping.', v_task_reward_code;
        END IF;
    END LOOP;

    RAISE NOTICE 'Rollback completed successfully.';

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '❌ Rollback failed: %', SQLERRM;
        RAISE EXCEPTION 'Transaction rolled back.';
END $$;
