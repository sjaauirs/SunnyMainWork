-- SUN-551 Rollback: Update task_completion_criteria_json for tasks 'Be mindful of what you eat','Meditate to boost your wellness','Rethink your drink','Strengthen your body','Get your z's','Step it up'
DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';
    v_language_code TEXT := 'en-US';
    v_task_headers TEXT[] := ARRAY[
        'Be mindful of what you eat',
        'Meditate to boost your wellness',
        'Rethink your drink',
        'Strengthen your body',
        'Get your z''s',
        'Step it up'
    ];
    v_json_payloads JSONB[] := ARRAY[
        '{
            "healthCriteria": {
                "requiredSleep": {
                "minSleepDuration": 0,
                "numDaysAtOrAboveMinDuration": 20
                },
                "healthTaskType": "SLEEP"
            },
            "selfReportType": "INTERACTIVE",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
        }'::jsonb,
        
        '{
            "healthCriteria": {
                "requiredSteps": 150,
                "healthTaskType": "STEPS"
            },
            "selfReportType": "INPUT",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
        }'::jsonb,

        '{
            "healthCriteria": {
                "requiredSleep": {
                "minSleepDuration": 0,
                "numDaysAtOrAboveMinDuration": 24
                },
                "healthTaskType": "SLEEP"
            },
            "selfReportType": "INTERACTIVE",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
        }'::jsonb,

        '{
            "healthCriteria": {
                "requiredSleep": {
                "minSleepDuration": 0,
                "numDaysAtOrAboveMinDuration": 8
                },
                "healthTaskType": "SLEEP"
            },
            "selfReportType": "INTERACTIVE",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
        }'::jsonb,

        '{
            "healthCriteria": {
                "requiredSleep": {
                "minSleepDuration": 0,
                "numDaysAtOrAboveMinDuration": 20
                },
                "healthTaskType": "SLEEP"
            },
            "selfReportType": "INTERACTIVE",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
        }'::jsonb,

        '{
            "healthCriteria": {
                "requiredSteps": 200000,
                "healthTaskType": "STEPS"
            },
            "selfReportType": "INPUT",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
        }'::jsonb
    ];

    v_task_id BIGINT;
BEGIN
    FOR i IN 1..array_length(v_task_headers, 1) LOOP
        -- Fetch the task_id
        SELECT d.task_id INTO v_task_id
        FROM task.task_detail d
        WHERE d.task_header = v_task_headers[i]
          AND d.tenant_code = v_tenant_code
          AND d.language_code = v_language_code
          AND d.delete_nbr = 0;

        IF v_task_id IS NOT NULL THEN
            UPDATE task.task_reward
            SET task_completion_criteria_json = v_json_payloads[i]
            WHERE task_id = v_task_id
              AND tenant_code = v_tenant_code
              AND delete_nbr = 0;

            RAISE NOTICE 'Updated task "%"', v_task_headers[i];
        ELSE
            RAISE NOTICE 'Task not found: %', v_task_headers[i];
        END IF;
    END LOOP;
END $$;

-- Rollback update for progress_detail in task.consumer_task table for 'Strengthen your body', 'Be mindful of what you eat', 'Rethink your drink'
DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';
    v_task_headers TEXT[] := ARRAY[
        'Strengthen your body',
        'Be mindful of what you eat',
        'Rethink your drink'
    ];
BEGIN
    UPDATE task.consumer_task ct
    SET progress_detail = jsonb_build_object(
        'detailType', 'SLEEP',
        'healthProgress', jsonb_build_object(
            'sleepTracking', jsonb_build_object(
                'minSleepDuration', 0,
                'numDaysAtOrAboveMinDuration',
                (progress_detail -> 'healthProgress' ->> 'totalUnits')::INT
            )
        )
    )
    FROM task.task_detail td
    WHERE ct.task_id = td.task_id
      AND td.task_header = ANY(v_task_headers)
      AND td.tenant_code = v_tenant_code
      AND ct.task_status IN ('IN_PROGRESS', 'COMPLETED')
	  AND ct.delete_nbr=0
	  AND td.delete_nbr=0
      AND ct.progress_detail ->> 'detailType' = 'OTHER'
      AND ct.progress_detail -> 'healthProgress' ? 'totalUnits';

    RAISE NOTICE 'Progress detail rolled back for tenant %', v_tenant_code;
END $$;

-- Rollback update for progress_detail in task.consumer_task table for 'Meditate to boost your wellness'
DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';
    v_task_headers TEXT[] := ARRAY[
        'Meditate to boost your wellness'
    ];
BEGIN
    UPDATE task.consumer_task ct
    SET progress_detail = jsonb_build_object(
        'detailType', 'STEPS',
        'healthProgress', jsonb_build_object(
            'totalSteps',
            (ct.progress_detail -> 'healthProgress' ->> 'totalUnits')::INT
        )
    )
    FROM task.task_detail td
    WHERE ct.task_id = td.task_id
      AND td.task_header = ANY(v_task_headers)
      AND td.tenant_code = v_tenant_code
      AND ct.task_status IN ('IN_PROGRESS', 'COMPLETED')
      AND ct.delete_nbr = 0
	  AND td.delete_nbr = 0
      AND ct.progress_detail ->> 'detailType' = 'OTHER'
      AND ct.progress_detail -> 'healthProgress' ? 'totalUnits';

    RAISE NOTICE 'Rollback applied for tenant % and task header: %', v_tenant_code, v_task_headers[1];
END $$;