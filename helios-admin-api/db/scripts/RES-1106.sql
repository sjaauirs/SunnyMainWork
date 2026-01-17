
-- ============================================================================
-- 🚀 Script: Insert into admin.script based
-- 📌 Purpose: Insert Script for watco chaining task
-- 🧑 Author  : Kawalpreet Kaur
-- 📅 Date    : 2025-11-24
-- 🧾 Jira    : RES-1106
-- ⚠️  Inputs: script_code, script_name, script_description, taskRewardCohorts JSON
-- ============================================================================
 DO $$
DECLARE
    v_script_code        VARCHAR := 'src-5e0ebae1a21c49b5af3591991e8e3842';

    -- Dynamically fetch the task reward code
    v_task_reward_code   VARCHAR;

    v_task_reward_cohorts_json TEXT;
    v_script_name        VARCHAR := 'WatcoChainTaskCompletion';
    v_script_description VARCHAR := 'Upon completion of one task should show another task. For that when task is completed, we''re adding a new cohort to the consumer so that the task mapped to the cohort will start showing to the consumer';

    v_script_json        JSONB := '{
        "args": [
            {"argName": "consumerDto", "argType": "Object"},
            {"argName": "taskRewardDetailDto", "argType": "Object"},
            {"argName": "cohortConsumerService", "argType": "Object"},
            {"argName": "taskService", "argType": "Object"}
        ],
        "result": {
            "ResultMap": "Object",
            "ResultCode": "number",
            "ErrorMessage": "string"
        }
    }';

    v_script_source TEXT;
    v_create_user  VARCHAR := 'Kumar';

BEGIN
    -- Fetch task reward code based on external code
    SELECT tr.task_reward_code
    INTO v_task_reward_code
    FROM task.task_reward tr
    WHERE tr.task_external_code = 'Learn_how_to_Enroll_&_Access_your_401(k)_2026'
      AND tr.delete_nbr = 0
    LIMIT 1;

    IF v_task_reward_code IS NULL THEN
        RAISE EXCEPTION '❌ No task_reward_code found for external code Learn_how_to_Enroll_&_Access_your_401(k)_2026';
    END IF;

    -- Build JSON payload dynamically
    v_task_reward_cohorts_json := format(
        '[{"task_reward_code": "%s", "chain_cohort": "Watco_chain_task_BOK_App"}]',
        v_task_reward_code
    );

    -- Build script source
    v_script_source := format($src$
function addConsumer(cohortConsumerService, consumerCode, tenantCode, cohortName) {
    var cohortConsumerRequestDto = {
        TenantCode: tenantCode, 
        ConsumerCode: consumerCode,
        CohortName: cohortName
    };

    var result = {
        ResultCode: 0,
        ErrorMessage: '',
        ResultMap: {}
    };

    var response = cohortConsumerService.AddConsumerCohort(cohortConsumerRequestDto);
    if (response.ErrorCode != null) {
        result.ResultCode = response.ErrorCode;
        result.ErrorMessage = response.ErrorMessage;
        return result;
    }
    return result;
}

function init(consumerDto, taskRewardDetailDto, cohortConsumerService, taskService) {
    try {
        var result = {
            ResultCode: 0,
            ErrorMessage: '',
            ResultMap: {}
        };

        const taskRewardCohorts = %s;
        const cohortsToAdd = taskRewardCohorts
            .filter(item => item.task_reward_code === taskRewardDetailDto.TaskReward.TaskRewardCode)
            .map(item => item.chain_cohort);

        if (cohortsToAdd) {
            result.ResultMap['addedConsumerCohort'] = [];
            cohortsToAdd.forEach(cohort => {
                var apiResult = addConsumer(cohortConsumerService, consumerDto.ConsumerCode, consumerDto.TenantCode, cohort);
                if (apiResult.ResultCode > 0) {
                    result.ResultCode = apiResult.ResultCode;
                    result.ErrorMessage = apiResult.ErrorMessage;
                    return result;
                }
                result.ResultMap['addedConsumerCohort'].push(cohort);
            });
        }
    } catch (error) {
        result.ResultCode = 1;
        result.ErrorMessage = error.message;
        result.ResultMap['StackTrace'] = error;
    }
    return result;
}

init(consumerDto, taskRewardDetailDto, cohortConsumerService, taskService);
$src$, v_task_reward_cohorts_json);

    -- Insert only if script not exists
    IF NOT EXISTS (
        SELECT 1 FROM admin.script WHERE script_code = v_script_code AND delete_nbr = 0
    ) THEN
        INSERT INTO admin.script (
            script_code, script_name, script_description,
            script_json, script_source, create_ts,
            create_user, delete_nbr
        )
        VALUES (
            v_script_code, v_script_name, v_script_description,
            v_script_json, v_script_source, CURRENT_TIMESTAMP,
            v_create_user, 0
        );
        RAISE NOTICE '✅ Script inserted successfully for script_code: %', v_script_code;
    ELSE
        RAISE NOTICE '⚠️ Script already exists. Skipped.';
    END IF;

EXCEPTION WHEN OTHERS THEN
    RAISE NOTICE '❌ Unexpected Error: %', SQLERRM;
END $$;
