--============================================================================
-- 🚀 Script    : Rollback do_not_show_in_ui cohort
-- 📌 Purpose   : Remove the cohort
-- 🧑 Author    : Kumar Sirikonda
-- 📅 Date      : 2025-09-24
-- 🧾 Jira      : RES-60
-- ⚠️ Inputs    : <HAP-TENANT-CODE>
-- 📤 Output    : https://github.com/SunnyRewards/helios-task-api/blob/develop/db/scripts/RES-60-Rollback.sql
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
 
-- ============================================================================
-- Declare input parameters
DO $$ 
DECLARE
    p_tenant_code VARCHAR := '<HAP-TENANT-CODE>';  -- Replace with actual tenant_code
    p_task_data JSONB := '[
        {"taskExternalCode": "comp_your_annu_well_visi", "taskthirdPartyCode": "ma_awv_26"},
        {"taskExternalCode": "comp_a_reco_colo_scre", "taskthirdPartyCode": "ma_col_26"},
        {"taskExternalCode": "comp_your_brea_canc_scre", "taskthirdPartyCode": "ma_mamm_26"},
        {"taskExternalCode": "comp_your_diab_eye_exam", "taskthirdPartyCode": "ma_eye_26"},
        {"taskExternalCode": "comp_your_a1c_test", "taskthirdPartyCode": "ma_a1c_26"},
        {"taskExternalCode": "get_your_flu_vacc", "taskthirdPartyCode": "ma_flu_26"}
    ]'::jsonb;  -- Replace with actual JSON data
    v_task_external_code VARCHAR;
    v_task_third_party_code VARCHAR;
    v_existing_count INTEGER;
    v_previous_third_party_code VARCHAR;
    rec JSONB;
BEGIN
    -- Notify start of rollback process
    RAISE NOTICE '🔄 Rolling back for tenant code: %', p_tenant_code;

    -- Loop through the JSON array using jsonb_array_elements
    FOR rec IN SELECT * FROM jsonb_array_elements(p_task_data)
    LOOP
        -- Extract values from the JSON object
        v_task_external_code := rec->>'taskExternalCode';
        v_task_third_party_code := rec->>'taskthirdPartyCode';

        -- Log the current task being processed for rollback
        RAISE NOTICE '🔍 Rolling back task: % - %', v_task_external_code, v_task_third_party_code;

        -- Check if the record exists
        SELECT COUNT(*) INTO v_existing_count
        FROM task.task_external_mapping
        WHERE tenant_code = p_tenant_code
          AND task_external_code = v_task_external_code
          AND delete_nbr = 0;

        -- If the record exists, delete it; otherwise, raise a notice
        IF v_existing_count > 0 THEN
            -- Delete the record if it exists
            RAISE NOTICE '✂️ Deleting task: % - %', v_task_external_code, v_task_third_party_code;

            DELETE FROM task.task_external_mapping
            WHERE tenant_code = p_tenant_code
              AND task_external_code = v_task_external_code
              AND delete_nbr = 0;

            -- Log the delete action
            RAISE NOTICE '✅ Deleted task: %', v_task_external_code;
        ELSE
            -- If no record found, raise a notice
            RAISE NOTICE '❌ Record not found for task: %', v_task_external_code;
        END IF;
    END LOOP;

    -- Notify the completion of the rollback process
    RAISE NOTICE '🎉 Rollback complete for tenant: %', p_tenant_code;
END $$;
