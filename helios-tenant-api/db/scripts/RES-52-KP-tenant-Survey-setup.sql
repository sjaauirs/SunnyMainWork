-- =================================================================================================================================
-- 🚀 Script    : Script for updating or inserting "Your Voice Matters" task reward code in tenant
-- 📌 Purpose   : Introduce or update a "Your Voice Matters" task for KP tenants only.
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-10-07
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-52
-- ⚠️ Inputs    : KP-TENANT-CODE and MAX count
-- 📤 Output    : Successfully updated or Inserted
-- 🔗 Script URL: NA
-- 📝 Notes     : Script needs to be executed in sequence. This is only for KP tenant script.
-- 🔢 Sequence Number: 2
-- ===================================================================================================================================
DO $$
DECLARE   
    -- <Input Parameters>                                       
    v_tenant_code TEXT := '<KP_TENANT_CODE>'; -- KP tenant only

    -- <Variable Declarations>
    v_task_external_code TEXT := 'your_voic_matt';
	v_max_survey_count   INT := 3;  -- Change this value when needed
    v_task_reward_code   TEXT;
    v_exists             BOOLEAN;
    v_existing_json      JSONB;
    v_new_json           JSONB;
    v_found_count        INT;
    v_current_count      INT;
    elem                 RECORD;  
BEGIN
    RAISE NOTICE '[Info] Starting update for tenant_code=%', v_tenant_code;

    -- Step 1: Get task reward code
    SELECT task_reward_code
    INTO v_task_reward_code
    FROM task.task_reward
    WHERE task_external_code = v_task_external_code 
      AND tenant_code = v_tenant_code
      AND delete_nbr = 0;

    -- If not found, raise error and stop
    IF v_task_reward_code IS NULL THEN
        RAISE NOTICE '[Error] Task reward code not found for task_external_code=%', v_task_external_code;
        RETURN;
    END IF;

    -- Step 2: Check if tenant exists
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

    -- Step 3: Get existing surveyTaskRewardCodes JSON
    SELECT tenant_option_json->'surveyTaskRewardCodes'
    INTO v_existing_json
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    IF v_existing_json IS NULL THEN
        v_existing_json := '[]'::jsonb;
    END IF;

    -- Step 4: Check if the task_reward_code already exists
    SELECT COUNT(*) 
    INTO v_found_count
    FROM jsonb_array_elements(v_existing_json) j
    WHERE j ? v_task_reward_code;

    IF v_found_count > 0 THEN
        -- Get the current count
        SELECT (j->>v_task_reward_code)::INT
        INTO v_current_count
        FROM jsonb_array_elements(v_existing_json) j
        WHERE j ? v_task_reward_code
        LIMIT 1;

        IF v_current_count = v_max_survey_count THEN
            RAISE NOTICE '[Info] Task reward code "%" already exists with same count=% for tenant=%',
                         v_task_reward_code, v_max_survey_count, v_tenant_code;
        ELSE
            -- Update only the count for existing task_reward_code
            v_new_json := '[]'::jsonb;

            FOR elem IN 
                SELECT value AS val FROM jsonb_array_elements(v_existing_json)
            LOOP
                IF elem.val ? v_task_reward_code THEN
                    v_new_json := v_new_json || jsonb_build_array(jsonb_build_object(v_task_reward_code, v_max_survey_count));
                ELSE
                    v_new_json := v_new_json || jsonb_build_array(elem.val);
                END IF;
            END LOOP;

            UPDATE tenant.tenant
            SET tenant_option_json = jsonb_set(
                    tenant_option_json,
                    '{surveyTaskRewardCodes}',
                    v_new_json,
                    true
                ),
                update_ts = NOW(),
                update_user = 'SYSTEM'
            WHERE tenant_code = v_tenant_code
              AND delete_nbr = 0;

            RAISE NOTICE '[Success] Updated count for task_reward_code=% from % to % for tenant_code=%',
                         v_task_reward_code, v_current_count, v_max_survey_count, v_tenant_code;
        END IF;

    ELSE
        -- Step 5: Add new entry if not exists
        v_new_json := v_existing_json || jsonb_build_array(jsonb_build_object(v_task_reward_code, v_max_survey_count));

        UPDATE tenant.tenant
        SET tenant_option_json = jsonb_set(
                tenant_option_json,
                '{surveyTaskRewardCodes}',
                v_new_json,
                true
            ),
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '[Success] Added new task_reward_code=% with count=% for tenant_code=%',
                     v_task_reward_code, v_max_survey_count, v_tenant_code;
    END IF;
END $$;
