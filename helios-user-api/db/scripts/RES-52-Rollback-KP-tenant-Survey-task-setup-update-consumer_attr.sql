
-- =================================================================================================================================
-- 🚀 Script    : Script for Rollback of huser.consumer table for consumer_attr 
-- 📌 Purpose   :  The purpose of this ticket is to rollback the new surveyTaskRewardCodes array to map the maximum time the user 
--                 skiped its survey.
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-10-08
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-52
-- ⚠️ Inputs    : KP_TENANT_CODE
-- 📤 Output    : Successfully Removes surveyTaskRewardCodes key from consumer_attr
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ===================================================================================================================================
DO $$
DECLARE
    -- <Input Parameters>
    v_tenant_code TEXT := '<KP_TENANT_CODE>';    -- KP tenant Only
       
    -- <Variable Declarations>
    v_updated_count INT := 0;
BEGIN
    RAISE NOTICE '[Info] Starting rollback for tenant_code=% -> removing key "surveyTaskRewardCodes"', v_tenant_code;

    -- Perform update: remove surveyTaskRewardCodes key
    UPDATE huser.consumer
    SET consumer_attr = consumer_attr - 'surveyTaskRewardCodes',
        update_ts = NOW(),
        update_user = 'SYSTEM'
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0
      AND consumer_attr ? 'surveyTaskRewardCodes';


    -- Get affected rows count
    GET DIAGNOSTICS v_updated_count = ROW_COUNT;

    IF v_updated_count > 0 THEN
        RAISE NOTICE '[Success] Removed key "surveyTaskRewardCodes" from % consumer record(s) for tenant_code=%',
            v_updated_count, v_tenant_code;
    ELSE
        RAISE NOTICE '[Info] No consumer records required rollback for tenant_code=% (key not found or already removed)',
            v_tenant_code;
    END IF;

    
    RAISE NOTICE '[Info] Rollback process completed';
END $$;
