-- ============================================================================
-- 🚀 Script    : Script for Clean up junk tasks for HAP-TENANT-CODE
-- 📌 Purpose   : To Soft delete junk task that are not related to HAP tenant
-- 🧑 Author    : Siva
-- 📅 Date      : 2025-09-26
-- 🧾 Jira      : RES-587
-- ⚠️ Inputs    : HAP-TENANT-CODE && array of valid HAP task_external_codes
-- 📤 Output    : Soft deletes the task rewards not present in the array
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ============================================================================


DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- 🔹 Input tenant_code
	
    v_valid_task_codes TEXT[] := ARRAY[
        'comp_your_annu_well_visi',
		'main_a_heal_bloo_pres',
		'comp_your_a1c_test',
		'comp_your_diab_eye_exam',
		'comp_a_reco_colo_scre',
		'comp_your_brea_canc_scre',
		'get_your_flu_vacc',
		'lear_abou_pres_home_deli',
		'conn_with_your_navi'
    ];  -- 🔹 Input array of valid task_external_codes
    
    v_total BIGINT;
    v_soft_deleted BIGINT;
BEGIN
    -- Count all tasks for tenant
    SELECT COUNT(*)
    INTO v_total
    FROM task.task_reward tr
    WHERE tr.tenant_code = v_tenant_code
      AND tr.delete_nbr = 0;

    RAISE NOTICE 'Total active task_reward records for tenant %: %', v_tenant_code, v_total;

    -- Perform soft delete for invalid task_external_codes
    UPDATE task.task_reward tr
    SET delete_nbr = tr.task_reward_id,
        update_ts = NOW(),
        update_user = 'RES-587'
    WHERE tr.tenant_code = v_tenant_code
      AND tr.delete_nbr = 0
      AND tr.task_external_code <> ALL(v_valid_task_codes);

    GET DIAGNOSTICS v_soft_deleted = ROW_COUNT;

    RAISE NOTICE 'Soft deleted % records for tenant % (not in provided array).', v_soft_deleted, v_tenant_code;

    RAISE NOTICE 'Operation complete.';
END $$;
