-- ============================================================================
-- 🚀 Script    : Script to Rollback task header for Complete your A1C task
-- 📌 Purpose   : Rollback to the previous task header for comp_your_a1c task
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-Oct-06
-- 🧾 Jira      : RES-598
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : Updates the task header in task.task_detail table
-- 🔗 Script URL: NA
-- 📝 Notes     : Execute if forward script executes
-- ============================================================================

DO $$
DECLARE
    -- Input parameters
    v_tenant_code            TEXT := '<HAP-TENANT-CODE>';       -- Input HAP Tenant Code

    v_task_external_code     TEXT := 'comp_your_a1c';           -- Old/existing external code
    v_new_task_external_code TEXT := 'comp_your_a1c_test';      -- New external code to update
    v_new_task_header        TEXT := 'Complete your A1C';  
    v_task_id                BIGINT;
    v_existing_code          TEXT;
BEGIN
    -- Step 1️: Check if record exists with either old or new external code
    SELECT tr.task_id, tr.task_external_code
    INTO v_task_id, v_existing_code
    FROM task.task_reward tr
    WHERE tr.tenant_code = v_tenant_code
      AND tr.task_external_code IN (v_task_external_code, v_new_task_external_code)
      AND tr.delete_nbr = 0
    LIMIT 1;

    IF v_task_id IS NULL THEN
        RAISE NOTICE '[Error]: No task_reward found for tenant_code=%, with external_code IN (%, %)',
                     v_tenant_code, v_task_external_code, v_new_task_external_code;
        RETURN;
    ELSE
        RAISE NOTICE '[Information]: task_reward found with task_id=% and current external_code="%"',
                     v_task_id, v_existing_code;
    END IF;

    -- Step 2️: Update task_external_code (only if it's not already the new one)
    IF v_existing_code <> v_task_external_code THEN
        UPDATE task.task_reward
        SET task_external_code = v_task_external_code
        WHERE tenant_code = v_tenant_code
          AND task_id = v_task_id
          AND delete_nbr = 0;

        RAISE NOTICE '[Information]: task_external_code Rollbacked from "%" to "%" for tenant_code=%, task_id=%',
                     v_existing_code, v_task_external_code, v_tenant_code, v_task_id;
    ELSE
        RAISE NOTICE '[Information]: task_external_code already up-to-date (=%) for tenant_code=%, task_id=%',
                     v_new_task_external_code, v_tenant_code, v_task_id;
    END IF;

    -- Step 3️: Update task_header in task_detail
    IF EXISTS (
        SELECT 1
        FROM task.task_detail td
        WHERE td.tenant_code = v_tenant_code
          AND td.task_id = v_task_id
          AND td.delete_nbr = 0
    ) THEN
        UPDATE task.task_detail
        SET task_header = v_new_task_header
        WHERE tenant_code = v_tenant_code
          AND task_id = v_task_id
          AND language_code = 'en-US'
          AND delete_nbr = 0;

        RAISE NOTICE '[Information]: task_detail.task_header updated successfully for tenant_code=%, task_id=%',
                     v_tenant_code, v_task_id;
    ELSE
        RAISE NOTICE '[Error]: No task_detail found for tenant_code=%, task_id=%',
                     v_tenant_code, v_task_id;
    END IF;

    RAISE NOTICE '[Success]: All updates completed successfully for tenant_code=%, task_id=%',
                 v_tenant_code, v_task_id;
END $$;
         
