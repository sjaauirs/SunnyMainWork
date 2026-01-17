-- =================================================================================================================================
-- 🚀 Script    : Script for insertion and updation of huser.consumer table for consumer_attr
-- 📌 Purpose   : The purpose of this ticket is to introduce a new surveyTaskRewardCodes array to map the maximum time the user 
--                 skiped its survey.
-- 🧑 Author    : Kawalpreet kaur
-- 📅 Date      : 2025-09-26
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-51
-- ⚠️ Inputs    : TENANT_CODE, TASK_REWARD_CODE
-- 📤 Output    : Successfully updated or Inserted
-- 🔗 Script URL: applicable after merge
-- 📝 Notes     : NA
-- ===================================================================================================================================
DO $$
DECLARE
    -- <Input Parameters>
    v_tenant_code TEXT := '<TENANT_CODE>';            
    v_task_reward_code TEXT := '<TASK_REWARD_CODE>';   
   
    -- <Variable Declarations>
    v_count INT := 0;                                
    v_updated_count INT := 0;
BEGIN
    RAISE NOTICE '[Info] Starting update  consumer for tenant_code=% (task_reward_code=% count=%)',
        v_tenant_code, v_task_reward_code, v_count;

    -- Perform update for ALL active consumers under this tenant
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

