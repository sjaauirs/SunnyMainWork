-- ============================================================================
-- 🚀 Script    : update task type if for trivia tasks 
-- 📌 Purpose   : For existing KP trivia tasks, the task_type_id was mapped wrong. This script is to update to correct task_type_id.
-- 🧑 Author    : Kumar Sirikonda
-- 📅 Date      : 2025-10-23
-- 🧾 Jira      : RES-761
-- ⚠️ Inputs    : v_tenant_code (TEXT), 
-- 📤 Output    : Success notice.
-- 🔗 Script URL:https://github.com/SunnyRewards/helios-task-api/blob/develop/db/scripts/RES-761-KP-tenant-update-task-type.sql
-- ============================================================================
DO $$
DECLARE
    v_tenant_code           VARCHAR := '<KP_TENANT_CODE>';       -- Input tenant_code
	
    v_task_external_codes   TEXT[] := ARRAY[
        'play_heal_triv_2026',
        'play_week_heal_triv_2026',
        'play_dail_heal_triv_2026'
    ];
    v_task_type_code        VARCHAR := 'tty-5c44328dce5a4b60ab79ab13e9253f27';  -- Task type code to be assigned
    v_task_type_id          BIGINT;
    v_task_id               BIGINT;
BEGIN
    -- Ensure task_type exists
    SELECT task_type_id
    INTO v_task_type_id
    FROM task.task_type
    WHERE task_type_code = v_task_type_code
      AND delete_nbr = 0;

    IF v_task_type_id IS NULL THEN
        RAISE NOTICE '⚠️ task_type "%" does not exist', v_task_type_code;
        RETURN;
    END IF;

    -- Update all task_ids in one shot based on task_external_codes
    UPDATE task.task
    SET task_type_id = v_task_type_id,
	update_ts = NOW(),
	update_user = 'SYSTEM'
    WHERE task_id IN (
        SELECT task_id
        FROM task.task_reward
        WHERE task_external_code = ANY(v_task_external_codes)
		AND delete_nbr = 0
    ) AND delete_nbr = 0;

    -- Optional: RAISE NOTICE to show how many records were updated
    RAISE NOTICE 'Updated task_type_id for tasks matching external codes';

END;
$$;
