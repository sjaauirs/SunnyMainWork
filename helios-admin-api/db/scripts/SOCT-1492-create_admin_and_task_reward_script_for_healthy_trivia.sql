-- ============================================================================
-- 🚀 Script: Insert into admin.script if script_code does not exist
-- 📌 Purpose: Adds a new script to assign cohorts based on person's DOB 
--             (even/odd logic) and task_reward_code, specific to a tenant
-- 🧑 Author  : Rakesh Pernati
-- 📅 Date    : 2025-07-29
-- 🧾 Jira    : SOCT-1492
-- ⚠️ Inputs  : Tenant Code
-- ============================================================================
-- 🔔 NOTE:
-- 📥 Tenant Code will be passed as an input
-- 🔐 Ensure script_code uniqueness before inserting

DO $$
DECLARE
    v_input_tenant_code TEXT := '<KP-TENANT-CODE>';  -- 👈 Input tenant code
    v_script_code TEXT := 'src-20809ec035f24caf936f65e8e354975b';
    v_script_name TEXT := 'DOBBasedCohortAssignment';
    v_script_description TEXT := 'Assigns cohort to consumer based on DOB even/odd logic using task_reward_code.';
    v_script_type TEXT := 'TASK_COMPLETE_POST';
    v_create_user TEXT := 'Kumar';
    v_task_external_code TEXT := 'play_heal_triv';
    v_task_reward_code TEXT;
    v_script_id INT;
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
    v_script_source_old TEXT;
    v_task_reward_cohorts_json JSONB;
    v_existing_json TEXT;
    v_existing_jsonb JSONB;
    elem JSONB;
BEGIN
    RAISE NOTICE '🚀 Starting script processing for tenant: %', v_input_tenant_code;

    -- 🔍 Get task_reward_code
    SELECT task_reward_code INTO v_task_reward_code
    FROM task.task_reward
    WHERE tenant_code = v_input_tenant_code
      AND delete_nbr = 0
      AND task_external_code = v_task_external_code;

    IF v_task_reward_code IS NULL THEN
        RAISE NOTICE '⚠️ No task_reward_code found for task_external_code: %', v_task_external_code;
        RETURN;
    END IF;

    -- Prepare cohort JSON
    v_task_reward_cohorts_json := jsonb_build_array(
        jsonb_build_object(
            'task_reward_code', v_task_reward_code,
            'odd_dob_cohort', 'adult18up+odd_dob',
            'even_dob_cohort', 'adult18up+even_dob'
        )
    );

    -- JS Source Template
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

    -- Replace placeholder JSON in script source
    v_script_source := replace(v_script_source, '$taskRewardCohorts$REPLACE_JSON$taskRewardCohorts$', v_task_reward_cohorts_json::TEXT);

    -- 🔍 Check if script already exists
    SELECT script_id, script_source INTO v_script_id, v_script_source_old
    FROM admin.script
    WHERE script_code = v_script_code
      AND delete_nbr = 0;

    IF v_script_id IS NULL THEN
        -- 🆕 Insert new script
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
        ) RETURNING script_id INTO v_script_id;

        RAISE NOTICE '✅ Script inserted for script_code: %', v_script_code;

    ELSE
       -- 🛠️ Script exists — Check if task_reward_code already exists
		IF POSITION(v_task_reward_code IN v_script_source_old) = 0 THEN
			-- 🔍 Extract existing JSON assigned to taskRewardCohorts
			v_existing_json := regexp_replace(v_script_source_old, '.*const taskRewardCohorts = ', '', 'g');
			v_existing_json := regexp_replace(v_existing_json, ';.*', '', 'g');

			-- ➕ Merge existing and new JSON
			v_existing_jsonb := v_existing_json::jsonb || v_task_reward_cohorts_json;

			-- 🧠 Prepare updated script source by replacing old JSON with new merged JSON
			v_script_source := replace(
				v_script_source_old,
				v_existing_json::text,
				v_existing_jsonb::text
			);

			-- 💾 Update the script with updated JSON
			UPDATE admin.script
			SET
				script_name = v_script_name,
				script_description = v_script_description,
				script_json = v_script_json,
				script_source = v_script_source,
				update_ts = CURRENT_TIMESTAMP,
				update_user = v_create_user
			WHERE script_id = v_script_id;

			RAISE NOTICE '🔄 Script updated with new task_reward_code: %', v_task_reward_code;
		ELSE
			RAISE NOTICE 'ℹ️ Script already contains task_reward_code: %, no update needed.', v_task_reward_code;
		END IF;

    END IF;

    -- 🔄 Handle tenant_task_reward_script link
    FOR elem IN SELECT * FROM jsonb_array_elements(v_task_reward_cohorts_json) LOOP
        v_task_reward_code := elem ->> 'task_reward_code';

        IF NOT EXISTS (
            SELECT 1 FROM admin.tenant_task_reward_script
            WHERE script_id = v_script_id
              AND task_reward_code = v_task_reward_code
              AND script_type = v_script_type
			  AND tenant_code = v_input_tenant_code
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
                'trs-' || REPLACE(gen_random_uuid()::TEXT, '-', ''),
                v_input_tenant_code,
                v_task_reward_code,
                v_script_type,
                v_script_id,
                CURRENT_TIMESTAMP,
                v_create_user,
                0
            );

            RAISE NOTICE '✅ Linked script to task_reward_code: %', v_task_reward_code;
        ELSE
            RAISE NOTICE '⚠️ Link already exists for task_reward_code: %', v_task_reward_code;
        END IF;
    END LOOP;

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '❌ Error: %', SQLERRM;
END $$;
