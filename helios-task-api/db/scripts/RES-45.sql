DO $$
DECLARE
    v_tenant_code TEXT := 'HAP_tenant_code'; -- 👈 pass tenant_code here
    v_mapping JSONB := '[ 
        { "task_external_code": "comp_your_annu_well_visi", "priority": 600 },
        { "task_external_code": "main_a_heal_bloo_pres",   "priority": 545 },
        { "task_external_code": "comp_your_a1c_test",      "priority": 530 },
        { "task_external_code": "comp_your_diab_eye_exam", "priority": 527 },
        { "task_external_code": "comp_a_reco_colo_scre",   "priority": 250 },
        { "task_external_code": "comp_your_brea_canc_scre","priority": 230 },
        { "task_external_code": "get_your_flu_vacc",       "priority": 350 },
        { "task_external_code": "lear_abou_pres_home_deli","priority": 330 },
        { "task_external_code": "conn_with_your_navi",     "priority": 300 }
    ]';
    v_record JSONB;
    v_updated_count INT;
    v_task_reward_code TEXT;

    -- Colon / BCS cohort variables
    v_cohort_name_colon TEXT := 'Due for Colon Screening';
    v_cohort_name_bcs   TEXT := 'Due for Breast Cancer Screening';
    v_priority_new      INT := 550;

    v_colon_id BIGINT;
    v_bcs_id   BIGINT;
    v_task_reward_code_colon TEXT;
    v_task_reward_code_bcs   TEXT;
BEGIN
    -- ---------------------------
    -- 1. Loop over JSON mapping for general task updates
    -- ---------------------------
    FOR v_record IN SELECT * FROM jsonb_array_elements(v_mapping)
    LOOP
        -- Update task.task_reward
        UPDATE task.task_reward tr
        SET priority = (v_record ->> 'priority')::INT
        WHERE tr.task_external_code = v_record ->> 'task_external_code'
          AND tr.tenant_code = v_tenant_code
          AND tr.delete_nbr = 0
        RETURNING tr.task_reward_code INTO v_task_reward_code;
        
        GET DIAGNOSTICS v_updated_count = ROW_COUNT;
        RAISE NOTICE 'task.task_reward: Updated task_external_code=% with priority=% (rows affected=%)',
            v_record ->> 'task_external_code',
            v_record ->> 'priority',
            v_updated_count;

        -- Update cohort.cohort_tenant_task_reward if task_reward_code exists
        IF v_task_reward_code IS NOT NULL THEN
            UPDATE cohort.cohort_tenant_task_reward ctr
            SET priority = (v_record ->> 'priority')::INT
            WHERE ctr.task_reward_code = v_task_reward_code
              AND ctr.tenant_code = v_tenant_code
              AND ctr.delete_nbr = 0;

            GET DIAGNOSTICS v_updated_count = ROW_COUNT;
            RAISE NOTICE 'cohort.cohort_tenant_task_reward: Updated task_reward_code=% with priority=% (rows affected=%)',
                v_task_reward_code,
                v_record ->> 'priority',
                v_updated_count;
        ELSE
            RAISE NOTICE '⚠️ No matching task_reward_code found for task_external_code=%',
                v_record ->> 'task_external_code';
        END IF;

        -- Reset variable for next loop
        v_task_reward_code := NULL;
    END LOOP;

    -- ---------------------------
    -- 2. Get Colon / BCS cohort IDs
    -- ---------------------------
    SELECT cohort_id
    INTO v_colon_id
    FROM cohort.cohort
    WHERE cohort_name = v_cohort_name_colon
      AND delete_nbr = 0
    LIMIT 1;

    SELECT cohort_id
    INTO v_bcs_id
    FROM cohort.cohort
    WHERE cohort_name = v_cohort_name_bcs
      AND delete_nbr = 0
    LIMIT 1;

    -- 3. Get task_reward_code for Colon / BCS
    SELECT task_reward_code
    INTO v_task_reward_code_colon
    FROM task.task_reward
    WHERE task_external_code = 'comp_a_reco_colo_scre'
      AND tenant_code = v_tenant_code
      AND delete_nbr = 0
    LIMIT 1;

    SELECT task_reward_code
    INTO v_task_reward_code_bcs
    FROM task.task_reward
    WHERE task_external_code = 'comp_your_brea_canc_scre'
      AND tenant_code = v_tenant_code
      AND delete_nbr = 0
    LIMIT 1;

    -- 4. Update cohort_tenant_task_reward for Colon
    IF v_colon_id IS NOT NULL AND v_task_reward_code_colon IS NOT NULL THEN
        UPDATE cohort.cohort_tenant_task_reward
        SET priority = v_priority_new
        WHERE cohort_id = v_colon_id
          AND task_reward_code = v_task_reward_code_colon
          AND tenant_code = v_tenant_code
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;
        RAISE NOTICE 'Updated Colon cohort priority to % (rows affected=%)', v_priority_new, v_updated_count;
    ELSE
        RAISE NOTICE '⚠️ Colon cohort or task_reward_code not found';
    END IF;

    -- 5. Update cohort_tenant_task_reward for BCS
    IF v_bcs_id IS NOT NULL AND v_task_reward_code_bcs IS NOT NULL THEN
        UPDATE cohort.cohort_tenant_task_reward
        SET priority = v_priority_new
        WHERE cohort_id = v_bcs_id
          AND task_reward_code = v_task_reward_code_bcs
          AND tenant_code = v_tenant_code
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;
        RAISE NOTICE 'Updated BCS cohort priority to % (rows affected=%)', v_priority_new, v_updated_count;
    ELSE
        RAISE NOTICE '⚠️ BCS cohort or task_reward_code not found';
    END IF;

END $$;
