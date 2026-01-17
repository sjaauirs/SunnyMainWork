DO $$
DECLARE
    -- 🔹 Replace with actual tenant code
    v_tenant_code TEXT := '<HAP-Tenant-Code>';

    v_data JSONB := '[
        {
            "taskExternalCode": "comp_your_a1c_test",
            "recurrenceDefinitionJson": {
                "schedules": [
                    { "startDate": "01-01", "expiryDate": "06-30" },
                    { "startDate": "07-01", "expiryDate": "12-31" }
                ],
                "recurrenceType": "SCHEDULE"
            }
        },
        {
            "taskExternalCode": "main_a_heal_bloo_pres",
            "recurrenceDefinitionJson": {
                "schedules": [
                    { "startDate": "01-01", "expiryDate": "03-31" },
                    { "startDate": "04-01", "expiryDate": "06-30" },
                    { "startDate": "07-01", "expiryDate": "09-30" },
                    { "startDate": "10-01", "expiryDate": "12-31" }
                ],
                "recurrenceType": "SCHEDULE"
            }
        }
    ]';

    task_record JSONB;
    v_task_code TEXT;
    v_task_json JSONB;
    v_updated_count INT;

BEGIN
    -- 🔹 Loop through each JSON element
    FOR task_record IN
        SELECT * FROM jsonb_array_elements(v_data)
    LOOP
        v_task_code := task_record->>'taskExternalCode';
        v_task_json := task_record->'recurrenceDefinitionJson';

        -- 🔹 Update recurrence_definition_json if matching record exists
        UPDATE task.task_reward
        SET recurrence_definition_json = v_task_json,
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE task_external_code = v_task_code
          AND tenant_code = v_tenant_code
          AND delete_nbr = 0
          AND is_recurring = TRUE;

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;

        IF v_updated_count > 0 THEN
            RAISE NOTICE '✅ Updated recurrence_definition_json for task: % (tenant: %)', v_task_code, v_tenant_code;
        ELSE
            RAISE NOTICE '⚠️ task_reward not found for task_external_code: % with tenant_code: % and is_recurring = TRUE', v_task_code, v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE '🎉 Recurrence update process completed!';
END $$;
