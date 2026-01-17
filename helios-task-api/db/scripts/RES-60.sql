--============================================================================
-- 🚀 Script    : Add do_not_show_in_ui cohort
-- 📌 Purpose   : Any task assgined to this cohort won't be shown in UI as we're not assigning this cohort to any consumer
-- 🧑 Author    : Kumar Sirikonda
-- 📅 Date      : 2025-09-24
-- 🧾 Jira      : RES-60
-- ⚠️ Inputs    : <HAP-TENANT-CODE>
-- 📤 Output    : https://github.com/SunnyRewards/helios-task-api/blob/develop/db/scripts/RES-60.sql
-- 🔗 Script URL: NA
-- 📝 Notes     : This script needs to be executed in the env during deployment before the tenant-import
 
-- ============================================================================
-- Declare input parameters
DO $$ 
DECLARE
	-- <Input Parameters>
    p_tenant_code VARCHAR := '<HAP-TENANT-CODE>';  -- Replace with actual tenant_code
	-- <Variable Declarations>
    p_task_data JSONB := '[
        {"taskExternalCode": "comp_your_annu_well_visi", "taskthirdPartyCode": "ma_awv_26"},
        {"taskExternalCode": "comp_a_reco_colo_scre", "taskthirdPartyCode": "ma_col_26"},
        {"taskExternalCode": "comp_your_brea_canc_scre", "taskthirdPartyCode": "ma_mamm_26"},
        {"taskExternalCode": "comp_your_diab_eye_exam", "taskthirdPartyCode": "ma_eye_26"},
        {"taskExternalCode": "comp_your_a1c_test", "taskthirdPartyCode": "ma_a1c_26"},
        {"taskExternalCode": "get_your_flu_vacc", "taskthirdPartyCode": "ma_flu_26"}
    ]'::jsonb;
    v_task_external_code VARCHAR;
    v_task_third_party_code VARCHAR;
    v_existing_count INTEGER;
    rec JSONB;
    v_reward_count INTEGER;
BEGIN
    -- Check if the tenant code exists in the tenant.tenant table
    IF NOT EXISTS (
        SELECT 1
        FROM tenant.tenant
        WHERE tenant_code = p_tenant_code AND delete_nbr = 0
    ) THEN
        -- If the tenant code does not exist, raise an exception
        RAISE EXCEPTION '❌ Tenant code "%" not found in tenant.tenant table', p_tenant_code;
    END IF;

    -- Notify start of processing
    RAISE NOTICE '🔄 Processing tenant code: %', p_tenant_code;

    -- Loop through the JSON array using jsonb_array_elements
    FOR rec IN SELECT * FROM jsonb_array_elements(p_task_data)
    LOOP
        -- Extract values from the JSON object
        v_task_external_code := rec->>'taskExternalCode';
        v_task_third_party_code := rec->>'taskthirdPartyCode';
		
		-- Check if a corresponding entry exists in task_reward with tenant_code, task_external_code, and delete_nbr = 0
        SELECT COUNT(*) INTO v_reward_count
        FROM task.task_reward
        WHERE tenant_code = p_tenant_code
          AND task_external_code = v_task_external_code
          AND delete_nbr = 0;

        -- If no entry exists in task_reward, skip the task and raise a notice
        IF v_reward_count = 0 THEN
            RAISE NOTICE '❌ No matching record found in task_reward for task: % - % with tenant: %. Skipping...', 
                         v_task_external_code, v_task_third_party_code, p_tenant_code;
            CONTINUE;  -- Skip to the next task in the loop
        END IF;

        -- Log the current task being processed
        RAISE NOTICE '🔍 Processing task: % - %', v_task_external_code, v_task_third_party_code;

        -- Check if the record already exists with delete_nbr = 0
        SELECT COUNT(*) INTO v_existing_count
        FROM task.task_external_mapping
        WHERE tenant_code = p_tenant_code
          AND task_external_code = v_task_external_code
          AND delete_nbr = 0;

        IF v_existing_count > 0 THEN
            -- If the record exists, update it
            RAISE NOTICE '✏️ Record exists. Updating task: %', v_task_external_code;

            UPDATE task.task_external_mapping
            SET
                task_third_party_code = v_task_third_party_code,
                update_ts = NOW(),
                update_user = 'SYSTEM'
            WHERE tenant_code = p_tenant_code
              AND task_external_code = v_task_external_code
              AND delete_nbr = 0;

            -- Log the update action
            RAISE NOTICE '✅ Updated task: % with new task_third_party_code: %', v_task_external_code, v_task_third_party_code;
        ELSE
            -- If the record does not exist, insert a new record
            RAISE NOTICE '➕ No record found. Inserting new task: %', v_task_external_code;

            INSERT INTO task.task_external_mapping(
                tenant_code,
                task_external_code,
                task_third_party_code,
                create_ts,
                create_user,
                delete_nbr
            )
            VALUES (
                p_tenant_code,
                v_task_external_code,
                v_task_third_party_code,
                NOW(),
                'SYSTEM',
                0  -- delete_nbr set to 0 (not deleted)
            );

            -- Log the insert action
            RAISE NOTICE '✅ Inserted new task: % with task_third_party_code: %', v_task_external_code, v_task_third_party_code;
        END IF;
    END LOOP;

    -- Notify the completion of the process
    RAISE NOTICE '🎉 Processing complete for tenant: %', p_tenant_code;
END $$;
