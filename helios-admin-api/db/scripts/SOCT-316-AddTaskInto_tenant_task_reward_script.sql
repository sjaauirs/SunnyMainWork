
-- ============================================================================
-- 🚀 Script: Insert into admin.tenant_task_reward_script if entry not exists for task_reward
-- 📌 Purpose: Adds a new chain task script with dynamic cohort mapping
-- 🧑 Author  : Kumar Sirikonda
-- 📅 Date    : 2025-05-23
-- 🧾 Jira    : SOCT-316
-- ⚠️  Inputs: tenant_code, task_reward_code list, script_code, script_type
-- ============================================================================
 
DO $$
DECLARE
    -- 🔸 Input Parameters
    v_tenant_code TEXT := 'ten-a468348402cd438ea9a1005ae2faedb6';
    v_task_reward_codes_list TEXT := 'trw-f8f535c7dbd44962909bbef1666924d1'; --coma seperated task reward codes
    v_script_code TEXT := 'src-5e0ebae1a21c49b5af3591991e873857';
    v_script_type TEXT := 'TASK_COMPLETE_POST';
	v_script_id BIGINT;
	v_task_reward_code TEXT;
 
BEGIN
	-- 🔍 Check for existing script_code
	SELECT script_id INTO v_script_id  
        FROM admin.script
        WHERE script_code = v_script_code AND delete_nbr = 0
        LIMIT 1; 

        IF v_script_id IS NULL THEN  
            RAISE EXCEPTION 'script_code "%" not found', v_script_code;
        END IF;
		
	-- Loop through each task reward code
    FOR v_task_reward_code IN SELECT unnest(string_to_array(v_task_reward_codes_list, ',')) 
    LOOP
		-- 🔍 Check for existing tenant_task_reward_script
		IF NOT EXISTS (
			SELECT 1
			FROM admin.tenant_task_reward_script
			WHERE script_id = v_script_id
				AND tenant_code = v_tenant_code
				AND script_type = v_script_type
				AND task_reward_code = v_task_reward_code
				AND delete_nbr = 0
		) THEN
			-- ✅ Insert new tenant_task_reward_script
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
				'trs-' || REPLACE(gen_random_uuid()::text, '-', ''),
				v_tenant_code,
				v_task_reward_code,
				v_script_type,
				v_script_id,
				CURRENT_TIMESTAMP,
				'SYSTEM',
				0
			); 
        RAISE NOTICE '✅ tenant_task_reward_script inserted successfully for script_code: %', v_script_code; 
    ELSE
        -- ❌ tenant_task_reward_script already exists
        RAISE NOTICE '⚠️ Skipping insert. tenant_task_reward_script already exists with script_code: %, task reward code:%', v_script_code, task_reward_code;
    END IF; 
    END LOOP;
EXCEPTION WHEN OTHERS THEN
    -- 🔥 Log any unexpected error
    RAISE NOTICE '❌ Unexpected error occurred: %', SQLERRM;
END $$;