-- ============================================================================
-- 🚀 Script: Insert into admin.script if script_code does not exist
-- 📌 Purpose: Adds a new cohort based on person's DOB even/odd and task_reward_code
-- 🧑 Author  : Kumar Sirikonda
-- 📅 Date    : 2025-06-04
-- 🧾 Jira    : SOCT-767
-- ⚠️  Inputs: script_code, script_name, script_description, taskRewardCohorts JSON
-- ============================================================================

DO $$
DECLARE
    v_script_code TEXT := 'src-20809ec035f24caf936f65e8e354975b';
    v_task_reward_cohorts_json JSONB := '[
        {
            "task_reward_code": "trw-1223c34d2fd241d1833fd07a74ad8f33",
            "odd_dob_cohort": "adult18up+odd_dob",
            "even_dob_cohort": "adult18up+even_dob"
        }
    ]';
    v_script_name TEXT := 'DOBBasedCohortAssignment';
    v_script_description TEXT := 'Assigns cohort to consumer based on DOB even/odd logic using task_reward_code.';
    v_script_json JSONB := '{
        "args": [
            {"argName": "consumerDto", "argType": "Object"},
            {"argName": "taskRewardDetailDto", "argType": "Object"},
            {"argName": "cohortConsumerService", "argType": "Object"},
            {"argName": "taskService", "argType": "Object"},
            {"argName": "personDto", "argType": "Object"}
        ],
        "result": {
            "ResultMap": "Object",
            "ResultCode": "number",
            "ErrorMessage": "string"
        }
    }';
    v_script_source TEXT;
	v_script_type TEXT := 'TASK_COMPLETE_POST';
    v_create_user  TEXT := 'Kumar';
    v_script_id INT;
    v_tenant_code TEXT;
    v_task_reward_code TEXT;
	elem JSONB;
BEGIN

    -- Safely embed JS as a TEXT block with JSON inside
    v_script_source := $src$
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

function getCohort(taskRewardCohorts, taskRewardCode, personDob) {
    const cohortEntry = taskRewardCohorts.find(item => item.task_reward_code === taskRewardCode);
    if (!cohortEntry) {
        throw new Error("Invalid task reward code");
    }

    const dobDay = personDob.Day;
    return dobDay % 2 === 0 ? cohortEntry.even_dob_cohort : cohortEntry.odd_dob_cohort;
}

function init(consumerDto, taskRewardDetailDto, cohortConsumerService, taskService, personDto) {
    try {
        var result = {
            ResultCode: 0,
            ErrorMessage: '',
            ResultMap: {}
        };

        const taskRewardCohorts = $taskRewardCohorts$REPLACE_JSON$taskRewardCohorts$;

        const cohortToAdd = getCohort(taskRewardCohorts, taskRewardDetailDto.TaskReward.TaskRewardCode, personDto.DOB);

        if (cohortToAdd) {
            result.ResultMap['addedConsumerCohort'] = [];
            var apiResult = addConsumer(cohortConsumerService, consumerDto.ConsumerCode, consumerDto.TenantCode, cohortToAdd);
            if (apiResult.ResultCode > 0) {
                result.ResultCode = apiResult.ResultCode;
                result.ErrorMessage = apiResult.ErrorMessage;
                return result;
            }
            result.ResultMap['addedConsumerCohort'].push(cohortToAdd);
        }
    } catch (error) {
        result.ResultCode = 1;
        result.ErrorMessage = error.message;
        result.ResultMap['StackTrace'] = error;
    }
    return result;
}

init(consumerDto, taskRewardDetailDto, cohortConsumerService, taskService, personDto);
$src$;

    -- Replace placeholder with escaped JSON
    v_script_source := replace(v_script_source, '$taskRewardCohorts$REPLACE_JSON$taskRewardCohorts$', v_task_reward_cohorts_json::TEXT);

    -- Check if script exists
    SELECT script_id INTO v_script_id
    FROM admin.script
    WHERE script_code = v_script_code
      AND delete_nbr = 0;
	  
	--Insert script if not exists
    IF v_script_id IS NULL THEN
        -- Insert script
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
        )
        RETURNING script_id INTO v_script_id;
        RAISE NOTICE '✅ Script inserted successfully for script_code: %', v_script_code;
    ELSE
        -- Update existing script
        UPDATE admin.script
        SET
            script_name = v_script_name,
            script_description = v_script_description,
            script_json = v_script_json,
            script_source = v_script_source,
            update_ts = CURRENT_TIMESTAMP,
            update_user = v_create_user
        WHERE script_id = v_script_id;

        RAISE NOTICE '🔄 Script updated for existing script_code: %', v_script_code;
    END IF;
	
	-- Insert missing tenant_task_reward_script entries
    FOR elem IN SELECT * FROM jsonb_array_elements(v_task_reward_cohorts_json) LOOP
		v_task_reward_code := elem ->> 'task_reward_code';

        -- Get tenant_code from task.task_reward
        SELECT tenant_code INTO v_tenant_code
        FROM task.task_reward
        WHERE task_reward_code = v_task_reward_code
          AND delete_nbr = 0
        LIMIT 1;

        IF v_tenant_code IS NULL THEN
            RAISE NOTICE '⚠️ No tenant_code found for task_reward_code: %', v_task_reward_code;
            CONTINUE;
        END IF;
		
        IF NOT EXISTS (
            SELECT 1 FROM admin.tenant_task_reward_script
            WHERE script_id = v_script_id
			  AND script_type = v_script_type
              AND task_reward_code = v_task_reward_code
              AND delete_nbr = 0
        ) THEN
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
				'trs-' || REPLACE(gen_random_uuid()::text, '-', ''),
				v_tenant_code,
                v_task_reward_code,
				v_script_type,
                v_script_id,
                CURRENT_TIMESTAMP,
                'Kumar',
                0
            );
            RAISE NOTICE '✅ Inserted tenant_task_reward_script for task_reward_code: %', elem ->> 'task_reward_code';
        ELSE
            RAISE NOTICE '⚠️ tenant_task_reward_script already exists for task_reward_code: %', elem ->> 'task_reward_code';
        END IF;
    END LOOP;

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '❌ Unexpected error occurred: %', SQLERRM;
END $$;
