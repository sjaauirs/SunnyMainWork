
 -- =================================================================================================================================
-- 🚀 Script    : Script for insertion and updation of your voice matters task 
-- 📌 Purpose   : The purpose of this query is to introduce a your voice matters task  for Qa tenants only for testing in QA env.
-- 🧑 Author    : Kawalpreet kaur
-- 📅 Date      : 2025-09-29
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-51
-- ⚠️ Inputs    : TENANT_CODE
-- 📤 Output    : Successfully updated or Inserted
-- 🔗 Script URL: only a Qa test script 
-- 📝 Notes     : Scripts needs to be executed in sequence. This is only for Qa tenant script, there is a RES-52 for KP config for this task
--sequence Number:1
-- ===================================================================================================================================
DO $$
DECLARE  
    -- <Input Parameters>
    v_tenant_code TEXT := '<KP_TENANT_CODE>'; --- QA env and qa KP tenant only	

    -- <Variable Declarations>
    v_cohort_name TEXT := 'Survey';
	v_task_name TEXT := 'Your Voice Matters';
	v_task_detail_name TEXT := 'Your Voice Matters';
    v_task_external_code TEXT := 'your_voic_matt';
    v_task_type_code TEXT := 'tty-86398dc3a77d4a3db7922e57b5b6d73c';
    v_task_reward_type_code TEXT := 'rtc-a5a943d3fc2a4506ab12218204d60805';
    v_reward_amount NUMERIC := 10;
	v_reward_type TEXT := 'MONETARY_DOLLARS';
 
	--Auto generated
    v_cohort_id BIGINT;
    v_cohort_code TEXT;
    v_task_code TEXT;
    v_task_type_id BIGINT;
    v_task_id BIGINT;
	v_task_detail_id BIGINT;
    v_task_reward_type_id BIGINT;
	v_task_reward_id BIGINT;
    v_task_reward_code TEXT;
	v_cohort_tenant_task_reward_id BIGINT;
	--schedule related variables		
    v_year INT := 2025;
    v_start_date DATE := make_date(v_year, 1, 1);
    v_end_date DATE := make_date(v_year, 12, 31);
    v_current_start DATE := v_start_date;
    v_current_end DATE;
    v_schedule JSONB := '[]'::jsonb;
 
BEGIN
    BEGIN
		--cohort
		SELECT cohort_id INTO v_cohort_id
            FROM cohort.cohort
            WHERE cohort_name = v_cohort_name AND delete_nbr = 0;
 
        IF v_cohort_id IS NULL THEN
			v_cohort_code := 'coh-' || REPLACE(gen_random_uuid()::text, '-', '');
 
			INSERT INTO cohort.cohort (
				cohort_code, cohort_name, cohort_description, parent_cohort_id, cohort_rule, 
				create_ts, update_ts, create_user, update_user, delete_nbr, cohort_enabled
			)
			SELECT v_cohort_code, v_cohort_name, 'This is a cohort for person eligible for survey.', NULL, '{}'::jsonb,
				NOW(), NULL, 'SYSTEM', NULL, 0, true
			WHERE NOT EXISTS (
				SELECT 1 FROM cohort.cohort WHERE cohort_name = v_cohort_name AND delete_nbr = 0
			)
			RETURNING cohort_id INTO v_cohort_id;
            RAISE NOTICE 'Inserted into cohort.cohort: cohort_id=%, cohort_code=%', v_cohort_id, v_cohort_code;
        ELSE
            RAISE NOTICE 'Cohort already exists: cohort_id=%', v_cohort_id;
        END IF;
 
        SELECT task_type_id INTO v_task_type_id  
        FROM task.task_type
        WHERE task_type_code = v_task_type_code AND delete_nbr = 0
        LIMIT 1;
 
        IF v_task_type_id IS NULL THEN  
            RAISE EXCEPTION 'task_type_code "%" not found', v_task_type_code;
        END IF;
 
		--task detail
		SELECT task_id INTO v_task_id
            FROM task.task
            WHERE task_name = v_task_name AND delete_nbr = 0;			
 
        IF v_task_id IS NULL THEN
			v_task_code := 'tsk-' || REPLACE(gen_random_uuid()::text, '-', '');
 
			INSERT INTO task.task (
				task_type_id, task_code, task_name, create_ts, update_ts, create_user, 
				update_user, delete_nbr, self_report, confirm_report, task_category_id, is_subtask
			)
			SELECT v_task_type_id, v_task_code, v_task_name, NOW(), NULL, 'SYSTEM', NULL, 0, false, false, NULL, false 
			WHERE NOT EXISTS (
				SELECT 1 FROM task.task WHERE task_name = v_task_name AND delete_nbr = 0
			)
			RETURNING task_id INTO v_task_id;
            RAISE NOTICE 'Inserted into task.task: task_id=%, task_code=%', v_task_id, v_task_code;
        ELSE
			RAISE NOTICE 'Task already exists: task_id=%', v_task_id;
        END IF;
 
		--task reward
		SELECT task_detail_id INTO v_task_detail_id
            FROM task.task_detail
            WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0 AND language_code='en-Us';
           
 
        IF v_task_detail_id IS NULL THEN
			INSERT INTO task.task_detail (
				task_id, language_code, task_header, task_description, terms_of_service_id, 
				create_ts, update_ts, create_user, update_user, delete_nbr, 
				task_cta_button_text, tenant_code
			)
			SELECT v_task_id, 'en-US', v_task_detail_name, '[{"type":"paragraph","data":{"text":"Help us shape the future of Kaiser permanente rewards. This short survey will only take few minutes and you will get $10 for sharing your thoughts."}}]', 1, 
				   NOW(), NULL, 'SYSTEM', NULL, 0, 'Get Started', v_tenant_code 
			WHERE NOT EXISTS (
				SELECT 1 FROM task.task_detail 
				WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0  AND language_code='en-US'
			)
			RETURNING task_detail_id INTO v_task_detail_id;
            RAISE NOTICE 'Inserted task_detail for task_id=%', v_task_id;
        ELSE
			RAISE NOTICE 'Task detail already exists: task_detail_id=%', v_task_detail_id;
        END IF;
        SELECT task_detail_id INTO v_task_detail_id
            FROM task.task_detail
            WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0 AND language_code='es';
 
        IF v_task_detail_id IS NULL THEN
			INSERT INTO task.task_detail (
				task_id, language_code, task_header, task_description, terms_of_service_id, 
				create_ts, update_ts, create_user, update_user, delete_nbr, 
				task_cta_button_text, tenant_code
			)
			SELECT v_task_id, 'es', 'Su voz importa ', '[{"type":"paragraph","data":{"text":"Ayúdenos a dar forma al futuro de las recompensas de Kaiser Permanente. Esta breve encuesta solo le llevará unos minutos y recibirá $10 por compartir sus opiniones."}}]', 1, 
				   NOW(), NULL, 'SYSTEM', NULL, 0, 'Empezar', v_tenant_code 
			WHERE NOT EXISTS (
				SELECT 1 FROM task.task_detail 
				WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0 And language_code='es'

			)
			RETURNING task_detail_id INTO v_task_detail_id;
            RAISE NOTICE 'Inserted task_detail for task_id=%', v_task_id;
        ELSE
			RAISE NOTICE 'Task detail already exists: task_detail_id=%', v_task_detail_id;
        END IF;
 
        SELECT reward_type_id INTO v_task_reward_type_id  
        FROM task.reward_type
        WHERE reward_type_code = v_task_reward_type_code
        LIMIT 1;
 
        IF v_task_reward_type_id IS NULL THEN  
            RAISE EXCEPTION 'reward_type_code "%" not found', v_task_reward_type_code;
        END IF;
 
		--task reward
		SELECT task_reward_code INTO v_task_reward_code
            FROM task.task_reward 
            WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0;
 
        IF v_task_reward_code IS NULL THEN
			v_task_reward_code := 'trw-' || REPLACE(gen_random_uuid()::text, '-', '');
			--prepare daily shedule json config
			WHILE v_current_start <= v_end_date LOOP
				v_current_end := LEAST(v_current_start + 6, v_end_date);
 
				v_schedule := v_schedule || jsonb_build_object(
					'startDate', to_char(v_current_start, 'MM-DD'),
					'expiryDate', to_char(v_current_end, 'MM-DD')
				);
 
				v_current_start := v_current_start + 7;
			END LOOP;
			INSERT INTO task.task_reward (
				task_id, reward_type_id, tenant_code, task_reward_code, reward, min_task_duration, 
				max_task_duration, expiry, priority, create_ts, update_ts, create_user, 
				update_user, delete_nbr, task_action_url, task_external_code, valid_start_ts, 
				is_recurring, recurrence_definition_json, self_report, task_completion_criteria_json, 
				confirm_report, task_reward_config_json, is_collection
			)
			SELECT v_task_id, v_task_reward_type_id, v_tenant_code, v_task_reward_code, 
				jsonb_build_object('rewardType', v_reward_type, 'rewardAmount', v_reward_amount, 'membershipType', 'MONETARY_DOLLARS'), 
				0, 0, '2100-01-01 00:00:00', -10, NOW(), NULL, 'SYSTEM', NULL, 0, NULL, v_task_external_code, 
				'2025-01-01 00:00:00', true, 
				'{"periodic": {"period": "MONTH","maxOccurrences": 1,"periodRestartDate": "1"},"recurrenceType": "PERIODIC"}'::jsonb, 
				-- jsonb_build_object(
				-- 	'schedules', v_schedule,
				-- 	'recurrenceType', 'SCHEDULE'
				-- ),
				true, NULL, false, '{}'::jsonb, false 
			WHERE NOT EXISTS (
				SELECT 1 FROM task.task_reward 
				WHERE task_id = v_task_id AND tenant_code = v_tenant_code AND delete_nbr = 0
			)
			RETURNING task_reward_code INTO v_task_reward_code;
            RAISE NOTICE 'Inserted task_reward: task_reward_code=%', v_task_reward_code;
        ELSE
			RAISE NOTICE 'Task reward already exists: task_reward_code=%', v_task_reward_code;
        END IF;
 
		--cohort tenant task reward
		SELECT cohort_tenant_task_reward_id INTO v_cohort_tenant_task_reward_id
            FROM cohort.cohort_tenant_task_reward 
            WHERE cohort_id = v_cohort_id AND tenant_code = v_tenant_code 
            AND task_reward_code = v_task_reward_code AND delete_nbr = 0;
 
        IF v_cohort_tenant_task_reward_id IS NULL THEN
			INSERT INTO cohort.cohort_tenant_task_reward (
				cohort_id, tenant_code, task_reward_code, recommended, priority, 
				create_ts, update_ts, create_user, update_user, delete_nbr
			)
			SELECT v_cohort_id, v_tenant_code, v_task_reward_code, true, -10, 
				NOW(), NULL, 'SYSTEM', NULL, 0 
			WHERE NOT EXISTS (
				SELECT 1 FROM cohort.cohort_tenant_task_reward 
				WHERE cohort_id = v_cohort_id AND tenant_code = v_tenant_code 
				AND task_reward_code = v_task_reward_code AND delete_nbr = 0
			)
			RETURNING cohort_tenant_task_reward_id INTO v_cohort_tenant_task_reward_id;
            RAISE NOTICE 'Linked cohort to task_reward successfully. cohort_tenant_task_reward_id=%', v_cohort_tenant_task_reward_id;
        ELSE
            RAISE NOTICE 'Link between cohort and task_reward already exists. cohort_tenant_task_reward_id=%', v_cohort_tenant_task_reward_id;
        END IF;
 
        RAISE NOTICE 'Script completed successfully.';
 
    EXCEPTION
        WHEN OTHERS THEN
            RAISE NOTICE 'Error occurred: %', SQLERRM;
            RAISE EXCEPTION 'Transaction rolled back due to error.';
    END;
END $$;

-- =================================================================================================================================
-- 🚀 Script    : Script for updat and updation of your voice matters task reward code in tenant
-- 📌 Purpose   : Introduce a "Your Voice Matters" task for QA tenants only (testing in QA env).
-- 🧑 Author    : Kawalpreet Kaur
-- 📅 Date      : 2025-09-29
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-51
-- ⚠️ Inputs    : TENANT_CODE
-- 📤 Output    : Successfully updated or Inserted
-- 🔗 Script URL: Only a QA test script 
-- 📝 Notes     : Scripts needs to be executed in sequence. This is only for Qa tenant script, there is a RES-52 for KP config for this task
--sequence Number:2
-- ===================================================================================================================================
DO $$
DECLARE   
    -- <Input Parameters>                                       
    v_tenant_code TEXT := '<KP_TENANT_CODE>'; ---- --- QA env and qa KP tenant only

    -- <Variable Declarations>
    v_task_external_code TEXT := 'your_voic_matt';
    v_task_reward_code   TEXT;
    v_max_survey_count   INT := 3;  
    v_exists             BOOLEAN;
BEGIN
    RAISE NOTICE '[Info] Starting update for tenant_code=%', v_tenant_code;

    -- Step 0: Get task reward code
    SELECT task_reward_code
    INTO v_task_reward_code
    FROM task.task_reward
    WHERE task_external_code = v_task_external_code and tenant_code = v_tenant_code
      AND delete_nbr = 0;

    -- If not found, raise error and stop
    IF v_task_reward_code IS NULL THEN
        RAISE NOTICE '[Error] Task reward code not found for task_external_code=%', v_task_external_code;
        RETURN;
    END IF;

    -- Step 1: Check if tenant exists
    SELECT EXISTS (
        SELECT 1
        FROM tenant.tenant
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
    ) INTO v_exists;

    IF NOT v_exists THEN
        RAISE NOTICE '[Error] No active tenant found with tenant_code=%', v_tenant_code;
        RETURN;
    END IF;

    -- Step 2: Perform update
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json,
        '{surveyTaskRewardCodes}',
        COALESCE(tenant_option_json->'surveyTaskRewardCodes','[]'::jsonb) || 
        jsonb_build_array(jsonb_build_object(v_task_reward_code, v_max_survey_count)),
        true
    ),
    update_ts = NOW(),
    update_user = 'SYSTEM'
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE '[Success] Updated tenant_code=% with surveyTaskRewardCode=% and count=%',
                 v_tenant_code, v_task_reward_code, v_max_survey_count;
END $$;

-- =================================================================================================================================
-- 🚀 Script    : Script for insertion and updation of your voice matters task reward code in consumer
-- 📌 Purpose   : The purpose of this query is to introduce a your voice matters task  for Qa tenants only for testing in QA env.
-- 🧑 Author    : Kawalpreet kaur
-- 📅 Date      : 2025-09-29
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-51
-- ⚠️ Inputs    : TENANT_CODE
-- 📤 Output    : Successfully updated or Inserted
-- 🔗 Script URL: only a Qa test script 
-- 📝 Notes     : Scripts needs to be executed in sequence. This is only for Qa tenant script, there is a RES-52 for KP config for this task
--sequence Number:3
-- ===================================================================================================================================

DO $$
DECLARE
    -- <Input Parameters>                                       
    v_tenant_code TEXT := '<KP_TENANT_CODE>'; --- QA env and qa KP tenant only

    -- <Variable Declarations>
	v_task_external_code TEXT := 'your_voic_matt';
	v_task_reward_code TEXT;
    v_count INT := 0;                                
    v_updated_count INT := 0;
BEGIN
    RAISE NOTICE '[Info] Starting update  consumer for tenant_code=% (task_reward_code=% count=%)',
        v_tenant_code, v_task_reward_code, v_count;
		
	  -- Step 0: Get task reward code
    SELECT task_reward_code
    INTO v_task_reward_code
    FROM task.task_reward
    WHERE task_external_code = v_task_external_code and tenant_code = v_tenant_code
      AND delete_nbr = 0;

    -- If not found, raise error and stop
    IF v_task_reward_code IS NULL THEN
        RAISE NOTICE '[Error] Task reward code not found for task_external_code=%', v_task_external_code;
        RETURN;
    END IF;
	
    UPDATE huser.consumer
    SET consumer_attr = jsonb_set(
                            COALESCE(consumer_attr, '{}'::jsonb),
                            '{surveyTaskRewardCodes}',  
                            jsonb_build_array(jsonb_build_object(v_task_reward_code, v_count)), 
                            true
                        ),
        update_ts = NOW(),
        update_user = 'SYSTEM'
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    -- Capture how many rows were updated
    GET DIAGNOSTICS v_updated_count = ROW_COUNT;

    IF v_updated_count > 0 THEN
        RAISE NOTICE '[Success] Updated % consumer record(s) for tenant_code=% with task_reward_code=% and count=%',
            v_updated_count, v_tenant_code, v_task_reward_code, v_count;
    ELSE
        RAISE NOTICE '[Info] No consumer records updated for tenant_code=% (maybe none exist or already in sync)',
            v_tenant_code;
    END IF;

  RAISE NOTICE '[Info] Update process completed';
END $$;	

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
-- 📝 Notes     : Scripts needs to be executed in sequence. This is only for Qa tenant script, there is a RES-52 for KP config for this task
--sequence Number:4
-- ===================================================================================================================================

DO $$
DECLARE

  -- <Input Parameters>                                       
    v_tenant_code TEXT := '<KP_TENANT_CODE>'; --- QA env and qa KP tenant only
	
  -- <Variable Declarations>
 v_task_external_codes TEXT[] := ARRAY[
        'reth_your_drin_2026',
        'medi_to_boos_your_well_2026',
        'get_your_z_s_2026',
        'step_it_up_2026'
    ];	      v_task_reward_cohorts_json TEXT;
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
-- 📝 Notes     : Scripts needs to be executed in sequence. This is only for Qa tenant script, there is a RES-52 for KP config for this task
--sequence Number:5
-- ===================================================================================================================================
DO $$
DECLARE
    -- Input parameter
    v_tenant_code TEXT := '<KP_TENANT_CODE>'; --- QA env and qa KP tenant only

       -- Variables declaration
    v_task_external_codes TEXT[] := ARRAY[
        'reth_your_drin_2026',
        'medi_to_boos_your_well_2026',
        'get_your_z_s_2026',
        'step_it_up_2026'
    ];

    -- Variables
    v_task_external_code TEXT;
    v_task_reward_code TEXT;
    v_script_code TEXT := 'src-4fdd3ae6573b44eda0d343a775a3350c';
    v_script_id BIGINT;
    v_create_user TEXT := 'SYSTEM';
    v_tenant_task_reward_script_code TEXT;
BEGIN
    -- Step 1: Get script_id
    SELECT script_id
    INTO v_script_id
    FROM admin.script
    WHERE script_code = v_script_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_script_id IS NULL THEN
        RAISE EXCEPTION '[Error] Script not found for script_code=%', v_script_code;
    END IF;

    -- Step 2: Loop over each external code
    FOREACH v_task_external_code IN ARRAY v_task_external_codes
    LOOP
        -- Fetch task_reward_code
        SELECT task_reward_code
        INTO v_task_reward_code
        FROM task.task_reward
        WHERE task_external_code = v_task_external_code
          AND tenant_code = v_tenant_code
          AND delete_nbr = 0
        LIMIT 1;

        IF v_task_reward_code IS NULL THEN
            RAISE NOTICE '[Error] Task reward code not found for task_external_code=%', v_task_external_code;
            CONTINUE; -- skip to next
        END IF;

        -- Generate unique code (GUID-style prefixed with "trs-")
        v_tenant_task_reward_script_code := 'trs-' || gen_random_uuid();

        -- Check if mapping exists
        IF EXISTS (
            SELECT 1
            FROM admin.tenant_task_reward_script
            WHERE tenant_code = v_tenant_code
              AND task_reward_code = v_task_reward_code
              AND script_id = v_script_id
              AND delete_nbr = 0
        ) THEN
            -- Update existing
            UPDATE admin.tenant_task_reward_script
            SET tenant_task_reward_script_code = v_tenant_task_reward_script_code,
                script_type   = 'TASK_COMPLETE_POST',
                update_ts     = CURRENT_TIMESTAMP,
                update_user   = v_create_user
            WHERE tenant_code = v_tenant_code
              AND task_reward_code = v_task_reward_code
              AND script_id = v_script_id
              AND delete_nbr = 0;

            RAISE NOTICE '[Info] Updated mapping for tenant_code=%, task_reward_code=%', 
                v_tenant_code, v_task_reward_code;
        ELSE
            -- Insert new
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

            RAISE NOTICE '[Info] Inserted mapping for tenant_code=%, task_reward_code=%', 
                v_tenant_code, v_task_reward_code;
        END IF;
    END LOOP;
END $$;

	