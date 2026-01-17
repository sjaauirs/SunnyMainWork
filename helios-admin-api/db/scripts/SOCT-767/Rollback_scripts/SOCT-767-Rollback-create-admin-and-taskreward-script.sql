-- ============================================================================
-- 🔁 Rollback Script for: DOBBasedCohortAssignment (script_code: src-20809ec035f24caf936f65e8e354975b)
-- 🧑 Author  : Kumar Sirikonda
-- 📅 Date    : 2025-06-06
-- 📄 Purpose: Deletes inserted/updated script and task_reward_script entries
-- ============================================================================

DO $$
DECLARE
    v_script_code TEXT := 'src-20809ec035f24caf936f65e8e354975b';
    v_script_id INT;
    v_task_reward_cohorts_json JSONB := '[
        {
            "task_reward_code": "trw-1223c34d2fd241d1833fd07a74ad8f33",
            "odd_dob_cohort": "adult18up+odd_dob",
            "even_dob_cohort": "adult18up+even_dob"
        }
    ]';
    v_task_reward_code TEXT;
    v_deleted_count INT;
    elem JSONB;
BEGIN
    -- Get the script_id
    SELECT script_id INTO v_script_id
    FROM admin.script
    WHERE script_code = v_script_code
      AND delete_nbr = 0;

    IF v_script_id IS NOT NULL THEN

        -- Delete from tenant_task_reward_script for each task_reward_code
        FOR elem IN SELECT * FROM jsonb_array_elements(v_task_reward_cohorts_json) LOOP
            v_task_reward_code := elem ->> 'task_reward_code';

            UPDATE admin.tenant_task_reward_script
			SET delete_nbr = tenant_task_reward_script_id, update_ts = NOW(), update_user = 'ROLLBACK'
            WHERE script_id = v_script_id
              AND task_reward_code = v_task_reward_code
              AND delete_nbr = 0;

            GET DIAGNOSTICS v_deleted_count = ROW_COUNT;
            IF v_deleted_count > 0 THEN
                RAISE NOTICE '🗑️ Deleted % row(s) from tenant_task_reward_script for task_reward_code: %', v_deleted_count, v_task_reward_code;
            ELSE
                RAISE NOTICE 'ℹ️ No matching rows found in tenant_task_reward_script for task_reward_code: %', v_task_reward_code;
            END IF;
        END LOOP;

        -- Delete the script from admin.script
        UPDATE admin.script
		SET delete_nbr = script_id, update_ts = NOW(), update_user = 'ROLLBACK'
        WHERE script_id = v_script_id
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_deleted_count = ROW_COUNT;
        IF v_deleted_count > 0 THEN
            RAISE NOTICE '🗑️ Deleted % row(s) from admin.script for script_code: %', v_deleted_count, v_script_code;
        ELSE
            RAISE NOTICE 'ℹ️ No script deleted for script_code: %', v_script_code;
        END IF;

    ELSE
        RAISE NOTICE 'ℹ️ Script with script_code % not found or already deleted.', v_script_code;
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '❌ Rollback failed with error: %', SQLERRM;
END $$;
