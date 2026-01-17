DO $$
DECLARE
    -- 🔹 List of all Navitus tenant codes (add/remove as needed)
    v_tenant_codes TEXT[] := ARRAY[
        '<NAVITUS-TENANT-CODE>',
        '<NAVITUS-TENANT-CODE-QA>'
    ];

    -- 🔹 JSON array input for categories to rollback (same categories used earlier)
    v_data JSONB := '[
        { "actionCategory": "Behavioral Health" },
        { "actionCategory": "Benefits" },
        { "actionCategory": "Clinical Care Gap" },
        { "actionCategory": "Company Culture" },
        { "actionCategory": "Condition Management" },
        { "actionCategory": "Financial Wellness" },
        { "actionCategory": "Health and Wellness" },
        { "actionCategory": "Pharmacy" },
        { "actionCategory": "Preventive Care" },
        { "actionCategory": "Virtual Care" }
    ]';

    -- 🔹 Loop variables
    v_tenant_code TEXT;
    rec JSONB;
    v_action_category TEXT;
    v_task_category_id BIGINT;
    v_existing_id BIGINT;
BEGIN
    -- Loop through each tenant code
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE '♻️ Rolling back tenant: %', v_tenant_code;

        -- Loop through each category record in JSON
        FOR rec IN SELECT * FROM jsonb_array_elements(v_data)
        LOOP
            v_action_category := rec->>'actionCategory';

            -- 1️⃣ Get matching task_category_id
            SELECT tc.task_category_id
            INTO v_task_category_id
            FROM task.task_category tc
            WHERE tc.task_category_name = v_action_category
              AND tc.delete_nbr = 0;

            IF v_task_category_id IS NULL THEN
                RAISE NOTICE '⚠️ Category not found: % (tenant=%)', v_action_category, v_tenant_code;
                CONTINUE;
            END IF;

            -- 2️⃣ Soft delete from tenant_task_category
            UPDATE task.tenant_task_category
            SET 
                delete_nbr = 1,
                update_ts = NOW(),
                update_user = 'SYSTEM-ROLLBACK'
            WHERE task_category_id = v_task_category_id
              AND tenant_code = v_tenant_code
              AND delete_nbr = 0;

            GET DIAGNOSTICS v_existing_id = ROW_COUNT;

            IF v_existing_id > 0 THEN
                RAISE NOTICE '🗑️ Soft-deleted category=% for tenant=%', v_action_category, v_tenant_code;
            ELSE
                RAISE NOTICE 'ℹ️ No active record found to delete for category=% (tenant=%)', v_action_category, v_tenant_code;
            END IF;
        END LOOP;
    END LOOP;

    RAISE NOTICE '✅ Rollback (soft delete) completed for all tenants.';
END $$;
