-- ============================================================================
-- Script: Update task_reward for specific tasks and tenant
-- Purpose: For each given task name, update self_report = TRUE and
--          set task_completion_criteria_json in task_reward table
--          for the specified tenant (where delete_nbr = 0)
-- Date: 14 July 2025
-- ============================================================================

DO $$
DECLARE
    tenant_code_input TEXT := '<NAVITUS-TENANT-CODE>'; -- Replace with actual tenant code

    task_metadata_array JSONB := '[
        {
            "taskName": "Track Your Sleep",
            "taskCompletionCriteriaJson": {
                "healthCriteria": {
                    "requiredSleep": {
                        "minSleepDuration": 420,
                        "numDaysAtOrAboveMinDuration": 20
                    },
                    "healthTaskType": "SLEEP"
                },
                "selfReportType": "INTERACTIVE",
                "completionPeriodType": "MONTH",
                "completionCriteriaType": "HEALTH"
            }
        },
        {
            "taskName": "Track Your Steps",
            "taskCompletionCriteriaJson": {
                "healthCriteria": {
                    "requiredSteps": 200000,
                    "healthTaskType": "STEPS"
                },
                "selfReportType": "INPUT",
                "completionPeriodType": "MONTH",
                "completionCriteriaType": "HEALTH"
            }
        }
    ]';

    item JSONB;
    taskName TEXT;
    task_criteria_json JSONB;
    v_task_id INT;
    v_rows_updated INT;
    updated_codes TEXT;
BEGIN
    FOR item IN SELECT * FROM jsonb_array_elements(task_metadata_array)
    LOOP
        taskName := item ->> 'taskName';
        task_criteria_json := item -> 'taskCompletionCriteriaJson';

        RAISE NOTICE '🛠️ Processing task: %', taskName;

        SELECT task_id INTO v_task_id
        FROM task.task
        WHERE task_name = taskName
          AND delete_nbr = 0;

        IF v_task_id IS NOT NULL THEN
            WITH updated AS (
                UPDATE task.task_reward
                SET self_report = TRUE,
                    task_completion_criteria_json = task_criteria_json,
                    update_ts = NOW(), 
                    update_user = 'SYSTEM'
                WHERE task_id = v_task_id
                  AND tenant_code = tenant_code_input
                  AND delete_nbr = 0
                RETURNING task_reward_code
            )
            SELECT 
                COUNT(*)::INT,
                string_agg(task_reward_code::TEXT, ', ')
            INTO v_rows_updated, updated_codes
            FROM updated;

            IF v_rows_updated > 0 THEN
                RAISE NOTICE '✅ Updated % row(s) for task: %; task_reward_code(s): [%]', v_rows_updated, taskName, updated_codes;
            ELSE
                RAISE NOTICE '⚠️ No matching records to update in task_reward for task: %', taskName;
            END IF;
        ELSE
            RAISE NOTICE '❌ Task not found in task.task table: %', taskName;
        END IF;
    END LOOP;
END $$;
