DO $$
DECLARE
    v_tenant_code TEXT := 'HAP_tenant_code'; -- 👈 pass tenant_code here
    v_mapping JSONB := '[
        { "task_external_code": "comp_your_annu_well_visi", "priority": 10 },
        { "task_external_code": "main_a_heal_bloo_pres",   "priority": 20 },
        { "task_external_code": "comp_your_a1c_test",      "priority": 30 },
        { "task_external_code": "comp_your_diab_eye_exam", "priority": 40 },
        { "task_external_code": "comp_a_reco_colo_scre",   "priority": 50 },
        { "task_external_code": "comp_your_brea_canc_scre","priority": 60 },
        { "task_external_code": "get_your_flu_vacc",       "priority": 70 },
        { "task_external_code": "lear_abou_pres_home_deli","priority": 80 },
        { "task_external_code": "conn_with_your_navi",     "priority": 90 }
    ]';
    v_record JSONB;
    v_updated_count INT;
BEGIN
    -- Loop over each JSON object and update task_reward
    FOR v_record IN SELECT * FROM jsonb_array_elements(v_mapping)
    LOOP
        UPDATE task.task_reward tr
        SET priority = (v_record ->> 'priority')::INT
        WHERE tr.task_external_code = v_record ->> 'task_external_code'
          AND tr.tenant_code = v_tenant_code
          AND tr.delete_nbr = 0;
        
        GET DIAGNOSTICS v_updated_count = ROW_COUNT;
        
        RAISE NOTICE 'Updated task_external_code=% with priority=% (rows affected=%)',
            v_record ->> 'task_external_code',
            v_record ->> 'priority',
            v_updated_count;
    END LOOP;
END $$;
