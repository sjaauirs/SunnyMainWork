-- =================================================================================================================================
-- 🚀 Script    : Script for insertion and updation  admin.tenant_task_reward_script
-- 📌 Purpose   : The purpose of this ticket is to make entry in admin.tenant_task_reward_script for mapping script for tenant task's.
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-10-07
-- 🧾 Jira      : https://sunnyrewards.atlassian.net/browse/RES-52
-- ⚠️ Inputs    : KP_TENANT_CODE 
-- 📤 Output    : Successfully updated or Inserted
-- 🔗 Script URL: applicable after merge
-- 📝 Notes     : Scripts needs to be executed in sequence. This is only for KP tenant script.
-- 🔢 Sequence Number: 5
-- ===================================================================================================================================
DO $$
DECLARE
    -- Input parameter
    v_tenant_code TEXT := '<KP_TENANT_CODE>'; --- KP tenant only

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
            WHERE task_reward_code = v_task_reward_code
            AND script_type = 'TASK_COMPLETE_POST'
            AND delete_nbr = 0
            )
            THEN
            -- Update existing
            UPDATE admin.tenant_task_reward_script
            SET tenant_task_reward_script_code = v_tenant_task_reward_script_code,
                update_ts     = CURRENT_TIMESTAMP,
                update_user   = v_create_user
            WHERE tenant_code = v_tenant_code
              AND task_reward_code = v_task_reward_code
              AND script_id = v_script_id
              AND script_type = 'TASK_COMPLETE_POST'
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
