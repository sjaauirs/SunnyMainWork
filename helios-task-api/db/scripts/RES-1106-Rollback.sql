--===============================================================================
-- Script:   ROLLBACK - Soft delete task categories created by RES-1106 / RES-124
-- Purpose:  Soft delete (delete_nbr = 1) of categories inserted/updated earlier
-- Author:   Kawalpreet Kaur
-- Date:     08-12-2025
--===============================================================================

DO
$$
DECLARE
    -- JSON array input (same as original INSERT script)
    v_input JSON := '[
        { "v_task_category_name": "Steps" },
        { "v_task_category_name": "Healthy eating" },
        { "v_task_category_name": "Trivia" },
        { "v_task_category_name": "Wellness" },
        { "v_task_category_name": "Strength" },
        { "v_task_category_name": "Shopping" },
        { "v_task_category_name": "Vaccine" },
        { "v_task_category_name": "Work" },
        { "v_task_category_name": "Sleep" },
        { "v_task_category_name": "Enrollment" },
        { "v_task_category_name": "Card" },
        { "v_task_category_name": "Health" },
        { "v_task_category_name": "Beneficiary" },
        { "v_task_category_name": "Positivity" }
    ]';

    v_item JSON;
    v_task_category_name TEXT;
    v_task_category_code TEXT;
    v_updated_count INT;

BEGIN
    -- Loop through each JSON object
    FOR v_item IN SELECT * FROM json_array_elements(v_input)
    LOOP
        -- Extract values
        v_task_category_name := v_item->>'v_task_category_name';

        -- Generate code using the same logic as original script
        v_task_category_code := 'tcc-' || md5(v_task_category_name);

        -- Soft delete (set delete_nbr = 1)
        UPDATE task.task_category
        SET delete_nbr = task_category_id,
            update_user = 'ROLLBACK',
            update_ts  = NOW()
        WHERE task_category_code = v_task_category_code
          AND task_category_name = v_task_category_name
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;

        IF v_updated_count > 0 THEN
            RAISE NOTICE '🗑️ Soft deleted: %', v_task_category_name;
        ELSE
            RAISE NOTICE '⚠️ No active record found to delete for: %', v_task_category_name;
        END IF;

    END LOOP;

    RAISE NOTICE '✔️ Rollback (soft delete) completed.';
END
$$;
