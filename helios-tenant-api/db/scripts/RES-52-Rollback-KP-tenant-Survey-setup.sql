-- =================================================================================================================================
-- 🚀 Script    : Script for Rollback of tenant.tenant table for tenant_option_json
-- 📌 Purpose   :  The purpose of this ticket is to rollback the new surveyTaskRewardCodes array to map the maximum time the user 
--                 skiped its survey.
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-10-08
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-52
-- ⚠️ Inputs    : KP_TENANT_CODE
-- 📤 Output    : Successfully updated or Inserted
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ===================================================================================================================================
DO $$
DECLARE
    -- <Input Parameters>
    v_tenant_code TEXT := '<KP_TENANT_CODE>';   -- KP tenant code only
  
    -- <Variable Declarations>
    v_exists BOOLEAN;
    v_has_key BOOLEAN;
BEGIN
    RAISE NOTICE '[Info] Starting rollback for tenant_code=%', v_tenant_code;

    -- Step 1: Check if tenant exists
    SELECT EXISTS (
        SELECT 1
        FROM tenant.tenant
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
    )
    INTO v_exists;

    IF NOT v_exists THEN
        RAISE NOTICE '[Error] No active tenant found with tenant_code=%', v_tenant_code;
        RETURN;
    END IF;

    -- Step 2: Check if key exists
    SELECT tenant_option_json ? 'surveyTaskRewardCodes'
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0
    INTO v_has_key;

    IF NOT v_has_key THEN
        RAISE NOTICE '[Info] Nothing to rollback - key "surveyTaskRewardCodes" not found for tenant_code=%', v_tenant_code;
        RETURN;
    END IF;

    -- Step 3: Remove the key
    UPDATE tenant.tenant
    SET tenant_option_json = tenant_option_json - 'surveyTaskRewardCodes',
        update_ts = NOW(),
        update_user = 'SYSTEM'
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE '[Success] Removed key "surveyTaskRewardCodes" for tenant_code=%', v_tenant_code;
END $$;
