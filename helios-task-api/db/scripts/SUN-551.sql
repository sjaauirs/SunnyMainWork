-- SUN-551: Update task_completion_criteria_json for tasks 'Be mindful of what you eat','Meditate to boost your wellness','Rethink your drink','Strengthen your body','Get your z's','Step it up'
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
				"unitType": "Days",
                "unitLabel": {
                    "en-US": "days",
                    "es": "días"
                },
                "requiredUnits": 20,
                "healthTaskType": "OTHER"
            },
            "selfReportType": "INPUT",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
        }'::jsonb,

        '{
            "healthCriteria": {
				"unitType": "Minutes",
                "unitLabel": {
                    "en-US": "minutes",
                    "es": "minutos"
                },
                "requiredUnits": 150,
                "healthTaskType": "OTHER"
            },
            "selfReportType": "INPUT",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
        }'::jsonb,

        '{
            "healthCriteria": {
				"unitType": "Days",
                "unitLabel": {
                    "en-US": "days",
                    "es": "días"
                },
                "requiredUnits": 24,
                "healthTaskType": "OTHER"
            },
            "selfReportType": "INPUT",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
        }'::jsonb,

        '{
            "healthCriteria": {
				"unitType": "Days",
                "unitLabel": {
                    "en-US": "days",
                    "es": "días"
                },
                "requiredUnits": 8,
                "healthTaskType": "OTHER"
            },
            "selfReportType": "INPUT",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
        }'::jsonb,

        '{
            "healthCriteria": {
                "requiredSleep": {
                    "minSleepDuration": 0,
                    "numDaysAtOrAboveMinDuration": 20
                },
				"unitType": "Days",
                "unitLabel": {
                    "en-US": "days",
                    "es": "días"
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
				"unitType": "Steps",
                "unitLabel": {
                    "en-US": "steps",
                    "es": "pasos"
                },
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

-- Update progress_detail in task.consumer_task table for 'Strengthen your body', 'Be mindful of what you eat', 'Rethink your drink'
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
        'detailType', 'OTHER',
        'healthProgress', jsonb_build_object(
            'totalUnits',
            (ct.progress_detail -> 'healthProgress' -> 'sleepTracking' ->> 'numDaysAtOrAboveMinDuration')::INT
        )
    )
    FROM task.task_detail td
    WHERE ct.task_id = td.task_id
      AND td.task_header = ANY(v_task_headers)
      AND td.tenant_code = v_tenant_code
      AND ct.task_status IN ('IN_PROGRESS', 'COMPLETED')
      AND ct.delete_nbr = 0
	  AND td.delete_nbr = 0
      AND ct.progress_detail ->> 'detailType' = 'SLEEP'
      AND ct.progress_detail -> 'healthProgress' -> 'sleepTracking' ? 'numDaysAtOrAboveMinDuration';

    RAISE NOTICE 'Progress detail updated for tenant %', v_tenant_code;
END $$;

-- Update progress_detail in task.consumer_task table for 'Meditate to boost your wellness'
DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';
    v_task_headers TEXT[] := ARRAY[
        'Meditate to boost your wellness'
    ];
BEGIN
    UPDATE task.consumer_task ct
    SET progress_detail = jsonb_build_object(
        'detailType', 'OTHER',
        'healthProgress', jsonb_build_object(
            'totalUnits',
            (ct.progress_detail -> 'healthProgress' ->> 'totalSteps')::INT
        )
    )
    FROM task.task_detail td
    WHERE ct.task_id = td.task_id
      AND td.task_header = ANY(v_task_headers)
      AND td.tenant_code = v_tenant_code
      AND ct.task_status IN ('IN_PROGRESS', 'COMPLETED')
      AND ct.delete_nbr = 0
	  AND td.delete_nbr = 0
      AND ct.progress_detail ->> 'detailType' = 'STEPS'
      AND ct.progress_detail -> 'healthProgress' ? 'totalSteps';

    RAISE NOTICE 'Progress detail updated for tenant % and task header: %', v_tenant_code, v_task_headers[1];
END $$;