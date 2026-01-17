-- =================================================================================================================================
-- 🚀 Script    : Script for insertion and updation for admin.script table and admin.tenant_task_reward_script
-- 📌 Purpose   : The purpose of this ticket is to create the script for adding consumer to the cohort based on the
--                 source script validation and admin.tenant_task_reward_script is used to create the mapping for which 
--                 tenant and task reward code this script is linked to.
-- 🧑 Author    : Kawalpreet kaur
-- 📅 Date      : 2025-09-26
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-51
-- ⚠️ Inputs    : TASK_REWARD_CODE , COHORT_NAME
-- 📤 Output    : Successfully updated or Inserted
-- 🔗 Script URL: applicable after merge
-- 📝 Notes     : This is a script to be executed in a sequence first for admin.script then 2nd for admin.tenant_task_reward_script
-- ===================================================================================================================================

DO $$
DECLARE
  -- <Input Parameters>
    v_task_reward_cohorts_json TEXT := '[{ "task_reward_code": "<TASK_REWARD_CODE>", "cohort": "<COHORT_NAME>"}]';  ------ eg: task reward code: trw-4b5dd6e2756a45a98bf08398de43fbbf for colon screening , cohort_name=Survey

  -- <Variable Declarations>

    v_script_name        VARCHAR := 'MappSurveyTaskToCohort';
    v_script_code        VARCHAR := 'src-4fdd3ae6573b44eda0d343a775a3350c';
    v_script_description VARCHAR := 'Upon completion of one task should show another task. For that when task is completed, we''re adding a new cohort to the consumer so that the task mapped to the cohort will start showing to the consumer';
    v_script_json        JSONB := '{
        "args": [
            {"argName": "consumerDto", "argType": "Object"},
            {"argName": "taskRewardDetailDto", "argType": "Object"},
            {"argName": "cohortConsumerService", "argType": "Object"},
            {"argName": "taskService", "argType": "Object"},
            {"argName": "consumerLoginService", "argType": "Object"}
        ],
        "result": {
            "ResultMap": "Object",
            "ResultCode": "number",
            "ErrorMessage": "string"
        }
    }';

    v_script_source TEXT;
    v_create_user   VARCHAR := 'SYSTEM';
BEGIN
    -- Construct the script source with dynamic cohort array
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

function checkConsumerEligibility(consumerLoginService, consumerCode, tenantCode) {
    var result = {
        ResultCode: 0,
        ErrorMessage: '',
        ResultMap: {}
    };

    // Eligibility window
    var cutoffLoginDate = new Date("2025-09-15T23:59:59Z");
    var consumerEngagementFromDate = new Date("2025-09-15T00:00:00Z"); 
    var consumerEngagementUntilDate = new Date("2025-10-31T23:59:59Z"); 

    // --- First login check ---
    var loginResponse = consumerLoginService.GetConsumerFirstLoginDate(consumerCode);
    if (loginResponse.ErrorCode != null) {
        result.ResultCode = loginResponse.ErrorCode;
        result.ErrorMessage = loginResponse.ErrorMessage;
        return result;
    }

    var firstLoginDate = new Date(loginResponse.LoginTs);
    if (firstLoginDate > cutoffLoginDate) {
        result.ResultCode = 1001;
        result.ErrorMessage = "Consumer not eligible: first login after Sept 15, 2025";
        return result;
    }

    // --- Engagement check ---
    var engagementRequest = {
        ConsumerCode: consumerCode,
        EngagementFrom: consumerEngagementFromDate.toISOString(),
        EngagementUntil: consumerEngagementUntilDate.toISOString()
    };

    var engagementResponse = consumerLoginService.GetConsumerEngagementDetail(engagementRequest);
    if (engagementResponse.ErrorCode != null) {
        result.ResultCode = engagementResponse.ErrorCode;
        result.ErrorMessage = engagementResponse.ErrorMessage;
        return result;
    }

    var hasEngagement = engagementResponse.HasEngagement;

    if (!hasEngagement) {
        result.ResultCode = 1002;
        result.ErrorMessage = "Consumer not eligible: no engagement between Sep 15 - Oct 31, 2025";
        return result;
    }

    result.ResultMap["IsEligible"] = true;
    return result;
}

function init(consumerDto, taskRewardDetailDto, cohortConsumerService, taskService, consumerLoginService) {
    var result = {
        ResultCode: 0,
        ErrorMessage: '',
        ResultMap: {}
    };

    try {
        var eligibilityResult = checkConsumerEligibility(
            consumerLoginService,
            consumerDto.ConsumerCode,
            consumerDto.TenantCode
        );

        if (eligibilityResult.ResultCode > 0) {
            return eligibilityResult;
        }

        if (!eligibilityResult.ResultMap.IsEligible) {
            result.ResultCode = 2001;
            result.ErrorMessage = "Consumer not eligible based on login/engagement rules";
            return result;
        }

        const taskRewardCohorts = %s;

        const cohortsToAdd = taskRewardCohorts
            .filter(item => item.task_reward_code === taskRewardDetailDto.TaskReward.TaskRewardCode)
            .map(item => item.cohort);
        
        if (cohortsToAdd && cohortsToAdd.length > 0) {
            result.ResultMap['addedConsumerCohort'] = [];
            for (const cohort of cohortsToAdd) {
                var apiResult = addConsumer(cohortConsumerService, consumerDto.ConsumerCode, consumerDto.TenantCode, cohort);
                if (apiResult.ResultCode > 0) {
                    result.ResultCode = apiResult.ResultCode;
                    result.ErrorMessage = apiResult.ErrorMessage;
                    return result;
                }
                result.ResultMap['addedConsumerCohort'].push(cohort);
            }
        }
    } catch (error) {
        result.ResultCode = 1;
        result.ErrorMessage = error.message;
        result.ResultMap['StackTrace'] = error;
    }

    return result;
}

init(consumerDto, taskRewardDetailDto, cohortConsumerService, taskService, consumerLoginService);
$src$, v_task_reward_cohorts_json);

    --  Check for existing script_code
    IF EXISTS (
        SELECT 1
        FROM admin.script
        WHERE script_code = v_script_code
          AND delete_nbr = 0
    ) THEN
        --  Update existing
        UPDATE admin.script
        SET script_name        = v_script_name,
            script_description = v_script_description,
            script_json        = v_script_json,
            script_source      = v_script_source,
            update_ts          = CURRENT_TIMESTAMP,
            update_user        = v_create_user
        WHERE script_code = v_script_code
          AND delete_nbr = 0;

        RAISE NOTICE '[Info] Script updated successfully for script_code: %', v_script_code;
    ELSE
        --  Insert new script
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
        RAISE NOTICE '[Info] Script inserted successfully for script_code: %', v_script_code; 
    END IF; 

EXCEPTION WHEN OTHERS THEN
    -- Log any unexpected error
    RAISE NOTICE '[Error] Unexpected error occurred: %', SQLERRM;
END $$;



-- =================================================================================================================================
-- 🚀 Script    : Script for insertion and updation  admin.tenant_task_reward_script
-- 📌 Purpose   : The purpose of this ticket is to make entry in admin.tenant_task_reward_script for mapping script for tenant task's.
-- 🧑 Author    : Kawalpreet kaur
-- 📅 Date      : 2025-09-26
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-51
-- ⚠️ Inputs    : TASK_REWARD_CODE , TENANT_CODE 
-- 📤 Output    : Successfully updated or Inserted
-- 🔗 Script URL: applicable after merge
-- 📝 Notes     : This is a script to be executed in a sequence first for admin.script then 2nd for admin.tenant_task_reward_script
-- ===================================================================================================================================
DO $$
DECLARE
    -- <Input Parameters>
     v_tenant_code TEXT := '<TENANT_CODE>';              
    v_task_reward_code TEXT := '<TASK_REWARD_CODE>';     

    -- <Variable Declarations>
    v_script_code TEXT := 'src-4fdd3ae6573b44eda0d343a775a3350c';
    v_tenant_task_reward_script_code TEXT := 'trs-ded485dbbd2b4e7dab23297eb1f0e992'; 

    v_script_id BIGINT;
    v_create_user TEXT := 'SYSTEM';
BEGIN
    -- Step 1: Get script_id from admin.script
    SELECT script_id
    INTO v_script_id
    FROM admin.script
    WHERE script_code = v_script_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_script_id IS NULL THEN
        RAISE EXCEPTION '[Error] Script not found for script_code=%', v_script_code;
    END IF;

    -- Step 2: Check if mapping already exists
    IF EXISTS (
        SELECT 1
        FROM admin.tenant_task_reward_script
        WHERE tenant_code = v_tenant_code
          AND task_reward_code = v_task_reward_code
          AND script_id = v_script_id
          AND delete_nbr = 0
    ) THEN
        -- Update existing row
        UPDATE admin.tenant_task_reward_script
        SET tenant_task_reward_script_code = v_tenant_task_reward_script_code,
            script_type   = 'TASK_COMPLETE_POST',  -- update type if needed
            update_ts     = CURRENT_TIMESTAMP,
            update_user   = v_create_user
        WHERE tenant_code = v_tenant_code
          AND task_reward_code = v_task_reward_code
          AND script_id = v_script_id
          AND delete_nbr = 0;

        RAISE NOTICE '[Info] Updated existing mapping for tenant_code=%, task_reward_code=%, script_id=%', 
            v_tenant_code, v_task_reward_code, v_script_id;

    ELSE
        -- Insert new row with passed tenant_task_reward_script_code
        INSERT INTO admin.tenant_task_reward_script (
            tenant_task_reward_script_code,
            tenant_code,
            task_reward_code,
            script_type,
            script_id,
            create_ts,
            create_user,
            delete_nbr
        ) VALUES (
            v_tenant_task_reward_script_code,
            v_tenant_code,
            v_task_reward_code,
            'TASK_COMPLETE_POST',
            v_script_id,
            CURRENT_TIMESTAMP,
            v_create_user,
            0
        );

        RAISE NOTICE '[Info] Inserted new mapping for tenant_code=%, task_reward_code=%, script_id=%', 
            v_tenant_code, v_task_reward_code, v_script_id;
    END IF;

END $$;

