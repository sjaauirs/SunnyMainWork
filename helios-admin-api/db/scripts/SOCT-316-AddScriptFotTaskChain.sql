-- ============================================================================
-- 🚀 Script: Insert into admin.script if script_code does not exist
-- 📌 Purpose: Adds a new chain task script with dynamic cohort mapping
-- 🧑 Author  : Kumar Sirikonda
-- 📅 Date    : 2025-05-23
-- 🧾 Jira    : SOCT-316
-- ⚠️  Inputs: script_code, script_name, script_description, taskRewardCohorts JSON
-- ============================================================================
 
DO $$
DECLARE
    -- 🔸 Input Parameters
    v_script_code        VARCHAR := 'src-5e0ebae1a21c49b5af3591991e873857';
	--Modify the below mapping json accordingly to map upon completion of what task_reward what cohort needs to be added to the consumer.
    v_task_reward_cohorts_json TEXT := '[{task_reward_code: "trw-f8f535c7dbd44962909bbef1666924d1", chain_cohort: "navitus_chain_task_cohort_trw-f8f535c7dbd44962909bbef1666924d1"}]';
    v_script_name        VARCHAR := 'ChainTaskCompletion';
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

    -- 🧩 Construct the script source with dynamic cohort array
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
 
    -- 🔍 Check for existing script_code
    IF NOT EXISTS (
        SELECT 1
        FROM admin.script
        WHERE script_code = v_script_code
          AND delete_nbr = 0
    ) THEN

        -- ✅ Insert new script
        INSERT INTO admin.script (
            script_code,
            script_name,
            script_description,
            script_json,
            script_source,
            create_ts,
            create_user,
            delete_nbr
        ) VALUES (
            v_script_code,
            v_script_name,
            v_script_description,
            v_script_json,
            v_script_source,
            CURRENT_TIMESTAMP,
            v_create_user,
            0
        ); 
        RAISE NOTICE '✅ Script inserted successfully for script_code: %', v_script_code; 
    ELSE
        -- ❌ Script already exists
        RAISE NOTICE '⚠️ Skipping insert. Script already exists with script_code: %', v_script_code;
    END IF; 
EXCEPTION WHEN OTHERS THEN
    -- 🔥 Log any unexpected error
    RAISE NOTICE '❌ Unexpected error occurred: %', SQLERRM;
END $$;
