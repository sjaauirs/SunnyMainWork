-- ==================================================================================================
-- 🚀 Script    : rollback task description for task external code 'take_a_brea_from_alco_in_marc_2026'
-- 🧑 Author    : Kawalpreet Kaur
-- 📅 Date      : 2025-11-10
-- 🧾 Jira      : SUN-936
-- ⚠️ Inputs    : KP tenant Code
-- 📤 Output    : rollback task.task_detail task description
-- 🔗 Script URL: 
-- 📝 Notes     : 
-- ==================================================================================================DO $$

DO $$
DECLARE
    -- 🔹 List of all tenant codes to process
    v_tenant_codes TEXT[] := ARRAY[
        '<KP-TENANT-CODE>',
        '<KP-TENANT-CODE-QA>'
    ];

    -- 🔹 Single task external code
    v_task_external_code TEXT := 'take_a_brea_from_alco_in_marc_2026';

    -- 🔹 JSON description string to update
    v_new_description TEXT := '[{"type":"paragraph","data":{"text":"Start the new year by giving your body a reset. Cutting out alcohol can improve your health, sleep, and energy. From January through March, hit pause on alcoholic drinks daily and get your monthly reward."}},{"type":"paragraph","data":{"text":"To earn your February reward, record alcohol-free days by April 10th."}}]';

    -- 🔹 Loop variables
    v_tenant_code TEXT;
    v_task_id BIGINT;
    v_rowcount INT;
BEGIN
    -- 1️⃣ Loop through each tenant and process individually
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE '🔍 Processing tenant=% for task_external_code=%', v_tenant_code, v_task_external_code;

        -- 2️⃣ Get task_id for this tenant and task external code
        SELECT tr.task_id
        INTO v_task_id
        FROM task.task_reward tr
        WHERE tr.task_external_code = v_task_external_code
          AND tr.delete_nbr = 0
          AND tr.tenant_code = v_tenant_code
        LIMIT 1;

        -- 3️⃣ If task_id not found, skip this tenant
        IF v_task_id IS NULL THEN
            RAISE NOTICE '⚠️ No task_id found for tenant=% | task_external_code=%', v_tenant_code, v_task_external_code;
            CONTINUE;
        END IF;

        -- 4️⃣ Update task_detail with the new description
        UPDATE task.task_detail td
        SET task_description = v_new_description,
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE td.task_id = v_task_id
          AND td.tenant_code = v_tenant_code and Language_code='en-US'
          AND td.delete_nbr = 0;

        -- 5️⃣ Log affected rows
        GET DIAGNOSTICS v_rowcount = ROW_COUNT;

        IF v_rowcount > 0 THEN
            RAISE NOTICE '✅ Updated task_description for tenant=% | task_id=%', v_tenant_code, v_task_id;
        ELSE
            RAISE NOTICE 'ℹ️ No matching task_detail found for tenant=% | task_id=%', v_tenant_code, v_task_id;
        END IF;
    END LOOP;

    RAISE NOTICE '🎉 Update complete for all tenants (task_external_code=%).', v_task_external_code;
END $$;
