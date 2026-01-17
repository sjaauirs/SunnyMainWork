DO $$
DECLARE
    -- 🔹 Replace with your tenant code
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- Input HAP tenant code

    -- 🔹 JSON array input
    v_data JSONB := '[
        { 
			"taskExternalCode": "comp_your_annu_well_visi",
			"actionCategory": "Condition Care",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Hap-ConditionCare.svg" }
		},
		{ 
			"taskExternalCode": "get_your_flu_vacc",
			"actionCategory": "Preventive Care",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Hap-PreventiveCare.svg" }
		},
        { 
			"taskExternalCode": "main_a_heal_bloo_pres",
			"actionCategory": "Condition Care",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Hap-ConditionCare.svg" }
		},
        { 
			"taskExternalCode": "comp_your_a1c_test",
			"actionCategory": "Clinical Care Gap",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Hap-ClinicalCareGap.svg" }
		},
        { 
			"taskExternalCode": "comp_your_diab_eye_exam",
			"actionCategory": "Clinical Care Gap",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Hap-ClinicalCareGap.svg" }
		},
        { 
			"taskExternalCode": "comp_a_reco_colo_scre",
			"actionCategory": "Clinical Care Gap",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Hap-ClinicalCareGap.svg" }
		},
        { 
			"taskExternalCode": "comp_your_brea_canc_scre",
			"actionCategory": "Clinical Care Gap",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Hap-ClinicalCareGap.svg" }
		},
        { 
			"taskExternalCode": "lear_abou_pres_home_deli",
			"actionCategory": "Pharmacy",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Hap-Pharmacy.svg" }
		},
        { 
			"taskExternalCode": "conn_with_your_navi",
			"actionCategory": "Benefits",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Hap-Benefits.svg" }
		}
    ]';

    -- Variables for each loop
    rec JSONB;
    v_task_external_code TEXT;
    v_action_category TEXT;
    v_task_id BIGINT;
    v_task_category_id BIGINT;
    v_updated_count INT;
    v_resource_json JSONB;
    v_existing_id BIGINT;
BEGIN
    -- Loop through each record in the JSON array
    FOR rec IN SELECT * FROM jsonb_array_elements(v_data)
    LOOP
        v_task_external_code := rec->>'taskExternalCode';
        v_action_category := rec->>'actionCategory';
        v_resource_json := rec->'resourceURL';

        -- 1️ Get task_id from task.task_reward
        SELECT tr.task_id
        INTO v_task_id
        FROM task.task_reward tr
        WHERE tr.task_external_code = v_task_external_code
          AND tr.tenant_code = v_tenant_code
          AND tr.delete_nbr = 0;

        IF v_task_id IS NULL THEN
            RAISE NOTICE '❌ Task not found for task_external_code=% and tenant_code=%', v_task_external_code, v_tenant_code;
            CONTINUE;
        END IF;

        -- 2️ Get task_category_id from task.task_category
        SELECT tc.task_category_id
        INTO v_task_category_id
        FROM task.task_category tc
        WHERE tc.task_category_name = v_action_category
          AND tc.delete_nbr = 0;

        IF v_task_category_id IS NULL THEN
            RAISE NOTICE '⚠️ Task category not found for category=% (Task=%)', v_action_category, v_task_external_code;
            CONTINUE;
        END IF;

        -- 3️ Update task.task with found category
        UPDATE task.task
        SET task_category_id = v_task_category_id,
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE task_id = v_task_id
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;

        IF v_updated_count > 0 THEN
            RAISE NOTICE '✅ Updated task_id=% (Task=%) with category=%', v_task_id, v_task_external_code, v_action_category;
        ELSE
            RAISE NOTICE '⚠️ No update performed for task_id=% (Task=%)', v_task_id, v_task_external_code;
        END IF;

        -- 4️ Insert or Update task.tenant_task_category
        SELECT ttc.tenant_task_category_id
        INTO v_existing_id
        FROM task.tenant_task_category ttc
        WHERE ttc.task_category_id = v_task_category_id
          AND ttc.tenant_code = v_tenant_code
          AND ttc.delete_nbr = 0;

        IF v_existing_id IS NOT NULL THEN
            -- Update existing record
            UPDATE task.tenant_task_category
            SET resource_json = v_resource_json,
                update_ts = NOW(),
                update_user = 'SYSTEM'
            WHERE tenant_task_category_id = v_existing_id;

            RAISE NOTICE '🔄 Updated tenant_task_category_id=% for category=% and tenant_code=%',
                v_existing_id, v_action_category, v_tenant_code;
        ELSE
            -- Insert new record
            INSERT INTO task.tenant_task_category (
                task_category_id,
                tenant_code,
                resource_json,
                create_ts,
                create_user,
                delete_nbr
            )
            VALUES (
                v_task_category_id,
                v_tenant_code,
                v_resource_json,
                NOW(),
                'SYSTEM',
                0
            );

            RAISE NOTICE '➕ Inserted new tenant_task_category for category=% and tenant_code=%',
                v_action_category, v_tenant_code;
        END IF;

    END LOOP;

    RAISE NOTICE '🎉 Task category + tenant_task_category update process completed!';
END $$;
