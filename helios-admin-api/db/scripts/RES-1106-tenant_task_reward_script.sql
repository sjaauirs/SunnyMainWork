-- ============================================================================
-- 🚀 Script: Insert into admin.tenant_task_reward_script based on Task external code for multiple tenants
-- 📌 Purpose: Auto pick task_reward_code using task external code and tenant list
-- 🧑 Author  : Kawalpreet Kaur
-- 📅 Date    : 2025-11-24
-- 🧾 Jira    : RES-1106
-- ⚠️  Inputs: script_code, script_name, script_description, taskRewardCohorts JSON
-- ============================================================================
 
DO $$
DECLARE
    -- 🔸 Inputs
    v_tenant_codes TEXT[] := ARRAY['<WATCO_TENANT_CODE>', '<WATCO_TENANT_CODE>'];
    v_task_external_code TEXT := 'Learn_how_to_Enroll_&_Access_your_401(k)_2026';
    v_script_code TEXT := 'src-5e0ebae1a21c49b5af3591991e8e3842';
    v_script_type TEXT := 'TASK_COMPLETE_POST';

    v_script_id BIGINT;
    v_task_reward_code TEXT;
    v_tenant_code TEXT;
BEGIN
    -- 🔍 Get script_id
    SELECT script_id INTO v_script_id
    FROM admin.script
    WHERE script_code = v_script_code AND delete_nbr = 0
    LIMIT 1;

    IF v_script_id IS NULL THEN
        RAISE EXCEPTION 'script_code "%" not found', v_script_code;
    END IF;

    -- 🔁 Loop through tenant list
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        
        -- 🎯 Fetch Task Reward Code based on external code
        SELECT t.task_reward_code
        INTO v_task_reward_code
        FROM task.task_reward t
        WHERE t.task_external_code = v_task_external_code
          AND t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0
        LIMIT 1;

        IF v_task_reward_code IS NULL THEN
            RAISE NOTICE '❌ task_reward_code not found for tenant %, external code "%"', v_tenant_code, v_task_external_code;
            CONTINUE;
        END IF;

        -- 🔍 Check existence
        IF NOT EXISTS (
            SELECT 1
            FROM admin.tenant_task_reward_script
            WHERE tenant_code = v_tenant_code
              AND task_reward_code = v_task_reward_code
              AND script_type = v_script_type
              AND script_id = v_script_id
              AND delete_nbr = 0
        ) THEN

            -- ✅ Insert
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
                'trs-' || replace(gen_random_uuid()::text,'-',''),
                v_tenant_code,
                v_task_reward_code,
                v_script_type,
                v_script_id,
                CURRENT_TIMESTAMP,
                'SYSTEM',
                0
            );

            RAISE NOTICE '✅ Inserted — tenant: %, task_reward: %, external code: %',
                v_tenant_code, v_task_reward_code, v_task_external_code;

        ELSE
            RAISE NOTICE '⚠️ Skipped — already exists for tenant: %, task_reward: %',
                v_tenant_code, v_task_reward_code;
        END IF;

    END LOOP;

END $$;
