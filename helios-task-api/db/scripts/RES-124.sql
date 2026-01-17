DO $$
DECLARE
    -- 🔹 List of all Navitus tenant codes (add/remove as needed)
    v_tenant_codes TEXT[] := ARRAY[
        '<NAVITUS-TENANT-CODE>',
        '<NAVITUS-TENANT-CODE-QA>'
    ];

    -- 🔹 JSON array input for categories and icons
    v_data JSONB := '[
        { "actionCategory": "Behavioral Health",   "resourceURL": { "taskIconUrl": "/assets/icons/Behavioral-Health.png" } },
        { "actionCategory": "Benefits",             "resourceURL": { "taskIconUrl": "/assets/icons/Benefits.png" } },
        { "actionCategory": "Clinical Care Gap",    "resourceURL": { "taskIconUrl": "/assets/icons/Clinical-Care-Gap.png" } },
        { "actionCategory": "Company Culture",      "resourceURL": { "taskIconUrl": "/assets/icons/Company-Culture.png" } },
        { "actionCategory": "Condition Management", "resourceURL": { "taskIconUrl": "/assets/icons/Condition-Management.png" } },
        { "actionCategory": "Financial Wellness",   "resourceURL": { "taskIconUrl": "/assets/icons/Financial-Wellness.png" } },
        { "actionCategory": "Health and Wellness",  "resourceURL": { "taskIconUrl": "/assets/icons/cooking.png" } },
        { "actionCategory": "Pharmacy",             "resourceURL": { "taskIconUrl": "/assets/icons/Pharmacy.png" } },
        { "actionCategory": "Preventive Care",      "resourceURL": { "taskIconUrl": "/assets/icons/Preventive-Care.png" } },
        { "actionCategory": "Virtual Care",         "resourceURL": { "taskIconUrl": "/assets/icons/Virtual-Care.png" } }
    ]';

    -- 🔹 Loop variables
    v_tenant_code TEXT;
    rec JSONB;
    v_action_category TEXT;
    v_task_category_id BIGINT;
    v_existing_id BIGINT;
    v_resource_json JSONB;
BEGIN
    -- Loop through each tenant code
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE '🏁 Processing tenant: %', v_tenant_code;

        -- Loop through each record in the JSON array
        FOR rec IN SELECT * FROM jsonb_array_elements(v_data)
        LOOP
            v_action_category := rec->>'actionCategory';
            v_resource_json := rec->'resourceURL';

            -- 1️⃣ Find matching task_category_id
            SELECT tc.task_category_id
            INTO v_task_category_id
            FROM task.task_category tc
            WHERE tc.task_category_name = v_action_category
              AND tc.delete_nbr = 0;

            IF v_task_category_id IS NULL THEN
                RAISE NOTICE '⚠️ Category not found: % (tenant=%)', v_action_category, v_tenant_code;
                CONTINUE;
            END IF;

            -- 2️⃣ Check if record exists for tenant + category
            SELECT ttc.tenant_task_category_id
            INTO v_existing_id
            FROM task.tenant_task_category ttc
            WHERE ttc.task_category_id = v_task_category_id
              AND ttc.tenant_code = v_tenant_code
              AND ttc.delete_nbr = 0;

            -- 3️⃣ Update or Insert accordingly
            IF v_existing_id IS NOT NULL THEN
                UPDATE task.tenant_task_category
                SET resource_json = v_resource_json,
                    update_ts = NOW(),
                    update_user = 'SYSTEM'
                WHERE tenant_task_category_id = v_existing_id;

                RAISE NOTICE '🔄 Updated category=% for tenant=% (ID=%)', 
                    v_action_category, v_tenant_code, v_existing_id;
            ELSE
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

                RAISE NOTICE '➕ Inserted category=% for tenant=%', 
                    v_action_category, v_tenant_code;
            END IF;
        END LOOP;
    END LOOP;

    RAISE NOTICE '🎉 All tenants processed successfully!';
END $$;
