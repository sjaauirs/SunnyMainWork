-- =================================================================================================================================
-- 🚀 Script    : Script for insertion and updation for admin.script table and admin.tenant_task_reward_script
-- 📌 Purpose   : The purpose of this ticket is to create the script for adding consumer to the cohort based on the
--                 source script validation and admin.tenant_task_reward_script is used to create the mapping for which 
--                 tenant and task reward code this script is linked to.
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-10-07
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-52
-- ⚠️ Inputs    : KP-TENANT-CODE, array of task_external_codes(took from RES-52 story description)
-- 📤 Output    : Successfully updated or Inserted
-- 🔗 Script URL: NA
-- 📝 Notes     : Scripts needs to be executed in sequence. This is only for KP tenant.
-- 🔢 Sequence Number: 4
-- ===================================================================================================================================

DO $$
DECLARE

  -- <Input Parameters>                                       
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- KP tenant only
	
  -- <Variable Declarations>
 v_task_external_codes TEXT[] := ARRAY[
        'reth_your_drin_2026',
        'medi_to_boos_your_well_2026',
        'get_your_z_s_2026',
        'step_it_up_2026'
    ];
    
    v_task_reward_cohorts_json TEXT;
    v_task_reward_code TEXT;


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
 SELECT json_agg(
               json_build_object(
                   'task_reward_code', tr.task_reward_code,
                   'cohort', 'Survey'
               )
           )::TEXT
    INTO v_task_reward_cohorts_json
    FROM task.task_reward tr
    WHERE tr.task_external_code = ANY(v_task_external_codes)
      AND tr.tenant_code = v_tenant_code
      AND tr.delete_nbr = 0;

    IF v_task_reward_cohorts_json IS NULL THEN
        RAISE NOTICE '[Error] No task reward codes found for tenant=%', v_tenant_code;
        RETURN;
    END IF;

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

    // -- Eligibility window
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