-- =================================================================================================================================
-- 🚀 Script    : Script for insertion and updation of tenant.tenant table for tenant_option_json
-- 📌 Purpose   : The purpose of this ticket is to introduce a new surveyTaskRewardCodes array to map the maximum time the Survey task should  
--                 appear.
-- 🧑 Author    : Kawalpreet kaur
-- 📅 Date      : 2025-09-26
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-51
-- ⚠️ Inputs    : TENANT_CODE, TASK_REWARD_CODE, max survey count
-- 📤 Output    : Successfully updated or Inserted
-- 🔗 Script URL: applicable after merge
-- 📝 Notes     : NA
-- ===================================================================================================================================
DO $$
DECLARE
    -- <Input Parameters>
    v_task_reward_code TEXT := '<SURVEY_TASK_REWARD_CODE>'; 
    v_max_survey_count INT := <3>;                                     
    v_tenant_code TEXT := '<TENANT_CODE>'; 

    -- <Variable Declarations>

    v_exists BOOLEAN;
BEGIN
    RAISE NOTICE '[Info] Starting update for tenant_code=%', v_tenant_code;

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
