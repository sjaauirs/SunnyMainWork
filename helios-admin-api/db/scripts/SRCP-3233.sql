/*
Template for creating the COSTCO script.

Instructions:
1. Populate `taskExternalCodes` for COSTCO tasks and `cohortNames` for cohorts in the `script_source` table.
2. For each task, create a corresponding entry in `tenant_task_reward_script`.

Fields that need to be updated:
   - Cohort Names: Replace placeholder names with actual cohort names.
     Example placeholders:
     - ***Coh0***
     - ***Coh1***
     - ***Coh2***
     - ***Coh3***
   
   - Task Codes: Replace placeholder codes with actual task external codes.
     Example placeholders:
     - ***Task01***
     - ***Task02***
     - ***Task03***
	 
	 - Task Reward Code : Preplace Reward Code PlaceHolders.
	 - ***TaskRewardCode01***
	 - ***TaskRewardCode02***
	 - ***TaskRewardCode03***
	 
3. Add All Consumers in Tenant to "Coh0" --update CohortId in the given Script 

Note: Replace all placeholders before executing the script.
*/


-- Insert script
INSERT INTO admin.script (
    script_code, 
    script_name, 
    script_description, 
    script_json, 
    script_source, 
    create_ts, 
    update_ts, 
    create_user, 
    update_user, 
    delete_nbr
)
VALUES (
    'src-d051dabee30b4ca4a547c5dbba706510',
    'CostcoTaskCompletePostScript',
    'This script executes post COSTCO task get completed.',
    '{
       "args": [
           { "argType": "Object", "argName": "consumerDto" },
           { "argType": "Object", "argName": "taskRewardDetailDto" },
           { "argType": "Object", "argName": "cohortConsumerService" },
           { "argType": "Object", "argName": "taskService" }
       ],
       "result": { 
           "ResultCode": "number",
           "ErrorMessage": "string", 
           "ResultMap": "Object"
       }
    }'::jsonb,
    $$ 
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

    function removeConsumer(cohortConsumerService, consumerCode, tenantCode, cohortName) {
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
        var response = cohortConsumerService.RemoveConsumerCohort(cohortConsumerRequestDto);
        if (response.ErrorCode != null) {
            result.ResultCode = response.ErrorCode;
            result.ErrorMessage = response.ErrorMessage;
            return result;
        }
        return result;
    }

    function softDeleteConsumerTask(taskService, consumerCode, tenantCode, taskExternalCode) {
        var softDeleteConsumerTaskRequestDto = {
            TenantCode: tenantCode, 
            ConsumerCode: consumerCode,
            TaskExternalCode: taskExternalCode
        };
        var result = {
            ResultCode: 0,
            ErrorMessage: '',
            ResultMap: {}
        };
        var response = taskService.SoftDeleteTask(softDeleteConsumerTaskRequestDto);
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
            var Coh0 = '***Coh0***'; 
            var Coh1 = '***Coh1***';
            var Coh2 = '***Coh2***';
            var Coh3 = '***Coh2***';

            var t00 = '***Task01***';
            var t01 = '***Task02***';
            var t02 = '***Task03***';

            const taskCodes = [t00, t01, t02];
            var completedTaskExternalCode = taskRewardDetailDto.TaskReward.TaskExternalCode;
            // If we have a task completed
            if (taskCodes.includes(completedTaskExternalCode)) {
                var apiResult = removeConsumer(cohortConsumerService, consumerDto.ConsumerCode, consumerDto.TenantCode, Coh0);
                if (apiResult.ResultCode > 0) {
                    result.ResultCode = apiResult.ResultCode;
                    result.ErrorMessage = apiResult.ErrorMessage;
                    return result;
                }

                result.ResultMap['removedConsumerCohort'] = Coh0;
                const incompleteTasks = taskCodes.filter(taskCode => taskCode !== completedTaskExternalCode);
                result.ResultMap['deletedConsumerTask'] = [];
                incompleteTasks.forEach(taskCode => { 
                    var apiResult = softDeleteConsumerTask(taskService, consumerDto.ConsumerCode, consumerDto.TenantCode, taskCode);
                    if (apiResult.ResultCode > 0 && apiResult.ResultCode != 404) {
                        result.ResultCode = apiResult.ResultCode;
                        result.ErrorMessage = apiResult.ErrorMessage;
                        return result;
                    }
                    if (apiResult.ResultCode == 0) {
                        result.ResultMap['deletedConsumerTask'].push(taskCode);
                    }
                });
                const cohortMapping = {
                    [t00]: [Coh2, Coh3],
                    [t01]: [Coh1, Coh3],
                    [t02]: [Coh1, Coh2]
                };

                const cohortsToAdd = cohortMapping[completedTaskExternalCode];
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
            }
        } catch (error) {
            result.ResultCode = 1;
            result.ErrorMessage = error.message;
            result.ResultMap['StackTrace'] = error;
        }
        return result;
    }

    init(consumerDto, taskRewardDetailDto, cohortConsumerService, taskService);
    $$,
    NOW(),          
    NULL,           
    'SYSTEM',       
    NULL,           
    0               
);



--- step 2 create entry  in tenant_task_reward_script table, we need  Task reward code of all costco main task


INSERT INTO admin.tenant_task_reward_script (
    tenant_task_reward_script_code, 
    tenant_code, 
    task_reward_code, 
    script_type, 
    script_id, 
    create_ts, 
    update_ts, 
    create_user, 
    update_user, 
    delete_nbr
)
VALUES (
    'trs-383b09288bb94cfc8b81a3f6e415de13',
    'ten-ecada21e57154928a2bb959e8365b8b4',
    '***TaskRewardCode01***',                    -- Update TaskRewardCode-1
    'TASK_COMPLETE_POST',
     (SELECT script_id 
     FROM admin.script 
     WHERE script_code = 'src-d051dabee30b4ca4a547c5dbba706510'),                   
    NOW(),           
    NULL,            
    'SYSTEM',        
    NULL,            
    0                
);

-- Task Reward Code for Task : ***Task02*** => ***TaskRewardCode02***

INSERT INTO admin.tenant_task_reward_script (
    tenant_task_reward_script_code, 
    tenant_code, 
    task_reward_code, 
    script_type, 
    script_id, 
    create_ts, 
    update_ts, 
    create_user, 
    update_user, 
    delete_nbr
)
VALUES (
    'trs-554d638135a74e62b40f8bf629040396',
    'ten-ecada21e57154928a2bb959e8365b8b4',
    '***TaskRewardCode02***',             -- Update TaskRewardCode-2
    'TASK_COMPLETE_POST',
    (SELECT script_id 
     FROM admin.script 
     WHERE script_code = 'src-d051dabee30b4ca4a547c5dbba706510'),                   
    NOW(),           
    NULL,            
    'system',        
    NULL,            
    0                
);

-- Task Reward Code for Task : ***Task03*** => ***TaskRewardCode03***
 
INSERT INTO admin.tenant_task_reward_script (
    tenant_task_reward_script_code, 
    tenant_code, 
    task_reward_code, 
    script_type, 
    script_id, 
    create_ts, 
    update_ts, 
    create_user, 
    update_user, 
    delete_nbr
)
VALUES (
    'trs-554d638135a74e62b40f8bf629040396',
    'ten-ecada21e57154928a2bb959e8365b8b4',
    '***TaskRewardCode03***',                      -- Update TaskRewardCode-3
    'TASK_COMPLETE_POST',
    (SELECT script_id 
     FROM admin.script 
     WHERE script_code = 'src-d051dabee30b4ca4a547c5dbba706510'),                   
    NOW(),           
    NULL,            
    'system',        
    NULL,            
    0                
);
 
 
 -- Step 3 :  Add All Consumers into Costo Main Cohort
 -- Note Change id for Costco main cohort
 -- Change  TenantCode
 
 
 WITH consumer_codes AS (
    SELECT consumer_code, tenant_code
    FROM huser.consumer 
    WHERE tenant_code = 'ten-03b771f6e344406aa9603a96aca9a527'  -- UPDATE TenantCode
      AND delete_nbr = 0
)
INSERT INTO cohort.cohort_consumer (
    cohort_id,
    tenant_code,
    consumer_code,
    create_ts,
    update_ts,
    create_user,
    update_user,
    delete_nbr
)
SELECT 
    91 AS cohort_id,    -- UPDATE Cohort Id for costco main cohort
    tenant_code,
    consumer_code,
    NOW() AS create_ts,
    NULL AS update_ts,
    'SYSTEM' AS create_user,
    NULL AS update_user,
    0 AS delete_nbr
FROM consumer_codes
WHERE NOT EXISTS (
    SELECT 1
    FROM cohort.cohort_consumer cc
    WHERE cc.tenant_code = consumer_codes.tenant_code
      AND cc.consumer_code = consumer_codes.consumer_code
      AND cc.cohort_id = 91  ---- UPDATE Cohort Id for costco main cohort
);