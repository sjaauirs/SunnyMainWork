-- ============================================================================
-- 🚀 Script    : Update Task Valid Start Timestamp for 2026
-- 📌 Purpose   : Update `valid_start_ts` in task.task_reward table for matching 
--                tenant_code, task_external_code, and delete_nbr = 0 records.
-- 🧑 Author    : Neel Kunchakurti
-- 📅 Date      : 2025-12-02
-- 🧾 Jira      : RES-1303
-- ⚠️ Inputs    : 
--                1. KP Tenant Codes array
-- 📤 Output    : Updates valid_start_ts column for matching records.
-- 🔗 Script URL: NA
-- 📝 Notes     : 
--                - Ensure you run this in a transaction.
--                - This script assumes valid tenant codes and task_external_codes exist.
--                - delete_nbr = 0 is used to filter active tasks.
-- ============================================================================

DO $$
DECLARE
    -- Input array of tenant codes
    v_tenant_codes TEXT[] := ARRAY[, '<KP-TENANT-CODE>'];
	v_tenant_code TEXT;
    v_task_external_code TEXT :='get_movi_2026';
    v_valid_start_ts TIMESTAMP := '2026-04-01 00:00:00';
    v_tenant TEXT;
    v_task JSON;
    v_task_header TEXT;
    v_task_id BIGINT;
BEGIN
     RAISE NOTICE '🔄 Starting updates for tenant codes: %', v_tenant_codes;

    -- Loop through tenant codes
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE '➡️ Processing tenant: %', v_tenant_code;

            -- Update statement
            UPDATE task.task_reward
            SET valid_start_ts = v_valid_start_ts,
                update_ts = NOW()
            WHERE tenant_code = v_tenant_code
              AND task_external_code = v_task_external_code
              AND delete_nbr = 0;

            IF FOUND THEN
                RAISE NOTICE '✅ Updated task % for tenant % with valid_start_ts = %', v_task_external_code, v_tenant_code, v_valid_start_ts;
            ELSE
                RAISE NOTICE '⚠️ No record found for tenant % and task_external_code %', v_tenant_code, v_task_external_code;
            END IF;
    END LOOP;

    RAISE NOTICE '🏁 All updates completed.';

END;
$$;
