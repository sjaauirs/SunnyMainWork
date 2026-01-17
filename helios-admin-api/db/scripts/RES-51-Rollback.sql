-- =================================================================================================================================
-- 🚀 Script    : Script for Rollback of admin.script table and admin.tenant_task_reward_script entries for survey task
-- 📌 Purpose   : Marks inserted records as deleted 
-- 🧑 Author    : Kawalpreet kaur
-- 📅 Date      : 2025-09-26
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-51
-- ⚠️ Inputs    : TENANT_CODE,TASK_REWARD_CODE
-- 📤 Output    : Successfully updated
-- 🔗 Script URL: applicable after merge
-- 📝 Notes     : NA
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
BEGIN
    -- Step 1: Soft delete from admin.script (only if exists & not already deleted)
    UPDATE admin.script
    SET delete_nbr = script_id,
        update_ts  = CURRENT_TIMESTAMP,
        update_user = 'SYSTEM'
    WHERE script_code = v_script_code
      AND delete_nbr = 0;

    GET DIAGNOSTICS v_script_id = ROW_COUNT;

    IF v_script_id > 0 THEN
        RAISE NOTICE '[Info] Rolled back admin.script for script_code=%', v_script_code;
    ELSE
        RAISE NOTICE '[Error] No active script found for script_code=% (nothing rolled back)', v_script_code;
    END IF;

    -- Step 2: Soft delete from admin.tenant_task_reward_script
    UPDATE admin.tenant_task_reward_script
    SET delete_nbr = tenant_task_reward_script_id,
        update_ts  = CURRENT_TIMESTAMP,
        update_user = 'SYSTEM'
    WHERE tenant_task_reward_script_code = v_tenant_task_reward_script_code
      AND tenant_code = v_tenant_code
      AND task_reward_code = v_task_reward_code
      AND delete_nbr = 0;

    GET DIAGNOSTICS v_script_id = ROW_COUNT;

    IF v_script_id > 0 THEN
        RAISE NOTICE '[Info] Rolled back admin.tenant_task_reward_script for tenant_code=%, task_reward_code=%',
            v_tenant_code, v_task_reward_code;
    ELSE
        RAISE NOTICE '[Error] No active mapping found for tenant_code=%, task_reward_code=% (nothing rolled back)',
            v_tenant_code, v_task_reward_code;
    END IF;
END $$;
