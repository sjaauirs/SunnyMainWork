-- ========================================================================================================
-- 🚀 Script    : Update Task Description by Tenant and External Code
-- 📌 Purpose   : Update 'task_description' in task.task_detail for a given tenant and task_external_code
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 09-Oct-2025
-- ⚙️ Inputs    : HAP TENANT CODE
-- 📤 Output    : Updates the task_detail.task_description
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ========================================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';       --  Input HAP tenant_code

    v_task_external_code TEXT := 'lear_abou_pres_home_deli';  
    v_task_description TEXT := '[
        {
            "type": "paragraph",
            "data": {
                "text": "We bring the pharmacy to you!\n\nSave time and money by having your prescriptions delivered right to your door.\n\nLearn more about home delivery so you never miss a dose. We''ll reward you for staying informed."
            }
        }
    ]'; 

    v_task_id BIGINT;
    v_exists BOOLEAN;
BEGIN
    -- Step 1: Fetch task_id from task.task_reward
    SELECT task_id INTO v_task_id
    FROM task.task_reward
    WHERE tenant_code = v_tenant_code
      AND task_external_code = v_task_external_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_task_id IS NULL THEN
        RAISE NOTICE '[Error] No task found for tenant_code=% and task_external_code=%', v_tenant_code, v_task_external_code;
        RETURN;
    END IF;

    -- Step 2: Check if task_detail record exists for en-US
    SELECT EXISTS (
        SELECT 1
        FROM task.task_detail
        WHERE task_id = v_task_id
          AND tenant_code = v_tenant_code
          AND language_code = 'en-US'
    ) INTO v_exists;

    IF v_exists THEN
        -- Update task_description
        UPDATE task.task_detail
        SET task_description = v_task_description,
            update_ts = NOW()
        WHERE task_id = v_task_id
          AND tenant_code = v_tenant_code
          AND language_code = 'en-US';

        RAISE NOTICE '[Info] Task description updated successfully for task_id=%', v_task_id;
    ELSE
        RAISE NOTICE '[Warning] No task_detail record found for en-US and task_id=%', v_task_id;
    END IF;

END $$;
