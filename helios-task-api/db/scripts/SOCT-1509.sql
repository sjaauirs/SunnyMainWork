/*
===============================================================================
 Script   : Update - task.task_detail (Header & Description)
 Author   : Vinod Ullaganti
 Date     : 2025-07-31
 Jira     : SOCT-1509
 Purpose  : 📝 Update task headers and replace 'Pilates' with 'pilates' in task
            descriptions for specific tasks in the task.task_detail table
            for a given tenant and language where the record is active.
===============================================================================
⚠️  Caution Before Execution:
    ✅ Ensure correct tenant_code and language_code are set
    ✅ Confirm task headers and descriptions align with source data
    🧪 Test thoroughly in QA before applying in UAT or PROD
    📌 Only active records (delete_nbr = 0) will be updated
===============================================================================
*/

DO $$
DECLARE
    -- 🔁 Replace with actual "Kaiser Permanente" tenant code
    v_tenant_code TEXT := '<KP-TENANT-CODE>';
    v_language_code   TEXT := 'en-US';
    v_update_user     TEXT := 'SYSTEM';
    v_i               INT;
    v_updated_count   INT := 0;

    -- Original Task Headers to match (case-insensitive)
    v_task_headers TEXT[] := ARRAY[
        'Complete the Total Health Assessment', -- 04
        'Track Your Sleep'-- 10
    ];

    -- New Task Headers (case-corrected or revised)
    v_new_task_headers TEXT[] := ARRAY[
        'Complete the total health assessment',-- 04
        'Track your sleep'-- 10
    ];

BEGIN
    -- 1️⃣ Update Task Headers
    FOR v_i IN 1..array_length(v_task_headers, 1) LOOP
        UPDATE task.task_detail
        SET
            task_header = v_new_task_headers[v_i],
            update_ts = NOW(),
            update_user = v_update_user
        WHERE tenant_code = v_tenant_code
          AND language_code = v_language_code
          AND LOWER(task_header) = LOWER(v_task_headers[v_i])
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;

        IF v_updated_count > 0 THEN
            RAISE NOTICE '[UPDATED] Task Header: "%", New Header: "%"',
                v_task_headers[v_i], v_new_task_headers[v_i];
        ELSE
            RAISE WARNING '[SKIPPED] No matching record found for: "%"', v_task_headers[v_i];
        END IF;
    END LOOP;

    -- Replace "Pilates" with "pilates" in task_description for a specific task
    UPDATE task.task_detail
    SET
        task_description = REPLACE(task_description, 'Pilates', 'pilates'),
        update_ts = NOW(),
        update_user = v_update_user
    WHERE tenant_code = v_tenant_code
      AND language_code = v_language_code
      AND LOWER(task_header) = LOWER('Strengthen your body') -- 09
      AND delete_nbr = 0;
	  
	-- Replace "Total Health Assessment" with "total health assessment" in task_description for a specific task
    UPDATE task.task_detail
    SET
        task_description = REPLACE(task_description, 'Total Health Assessment', 'Total health assessment'),
        update_ts = NOW(),
        update_user = v_update_user
    WHERE tenant_code = v_tenant_code
      AND language_code = v_language_code
      AND LOWER(task_header) = LOWER('Complete the Total Health Assessment')  -- 04
      AND delete_nbr = 0;

    RAISE NOTICE '✔️ Header and description update process completed.';
	  
END $$;
