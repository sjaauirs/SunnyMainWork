-- ============================================================================
-- 🚀 Script    : Update Spanish Task Headers for Multiple Tenants
-- 📌 Purpose   : 
--      Reads an input JSON array of (task_external_code, task_header) and an
--      array of tenant_codes.  
--      For each tenant + task_external_code:
--        1. Fetches task_id from task.task_reward (delete_nbr = 0)
--        2. Updates task_header in task.task_detail for language='es'
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-11-19
-- 🧾 Jira      : SUN-1174
-- ⚠️ Inputs    : Array of KP-TENANT-CODES
--        - v_tasks_json     : JSON containing array_task_external_codes
-- 📤 Output    : Updates rows in task.task_detail
-- 🔗 Script URL: NA
-- 📝 Notes     :
--        - Will only update Spanish language entries (language_code = 'es')
--        - Skips tasks/tenants not found and logs warnings
-- ============================================================================
DO
$$
DECLARE
    -- Input array of tenant codes
    v_tenant_codes TEXT[] := ARRAY['<KP-TENANT-CODE>', '<KP-TENANT-CODE>'];

    -- Input JSON array
    v_tasks_json JSON := '{
        "array_task_external_codes":[
            {
                "task_external_code" : "take_a_brea_from_alco_in_janu_2026",
                "task_header": "Enero: Tómese un descanso del alcohol"
            },
            {
                "task_external_code" : "take_a_brea_from_alco_in_febr_2026",
                "task_header": "Febrero: Tómese un descanso del alcohol"
            },
            {
                "task_external_code" : "take_a_brea_from_alco_in_marc_2026",
                "task_header": "Marzo: Tómese un descanso del alcohol"
            }
        ]
    }';

    v_tenant TEXT;
    v_task JSON;
    v_task_external_code TEXT;
    v_task_header TEXT;
    v_task_id BIGINT;
BEGIN
    -- Loop tenants
    FOREACH v_tenant IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE 'Processing tenant: %', v_tenant;

        -- Loop JSON array items
        FOR v_task IN SELECT * FROM json_array_elements(v_tasks_json->'array_task_external_codes')
        LOOP
            v_task_external_code := v_task->>'task_external_code';
            v_task_header        := v_task->>'task_header';

            RAISE NOTICE '  → Processing task_external_code: %', v_task_external_code;

            -- Step 1: Fetch task_id from task.task_reward
            SELECT tr.task_id INTO v_task_id
            FROM task.task_reward tr
            WHERE tr.tenant_code = v_tenant
              AND tr.task_external_code = v_task_external_code
              AND tr.delete_nbr = 0
            LIMIT 1;

            IF v_task_id IS NULL THEN
                RAISE NOTICE 'No task_reward found → (tenant=% task_external_code=%)', 
                                v_tenant, v_task_external_code;
                CONTINUE;
            END IF;

            RAISE NOTICE 'Found task_id: %', v_task_id;

            -- Step 2: Update task.task_detail for Spanish records
            UPDATE task.task_detail td
            SET task_header = v_task_header,
                update_ts = NOW()
            WHERE td.tenant_code = v_tenant
              AND td.task_id = v_task_id
              AND td.language_code = 'es'
              AND td.delete_nbr = 0;

            IF FOUND THEN
                RAISE NOTICE 'Updated task_header (language=es) → %', v_task_header;
            ELSE
                RAISE NOTICE 'No task_detail record found (tenant=% task_id=% lang=es)', 
                              v_tenant, v_task_id;
            END IF;

        END LOOP;
    END LOOP;

END;
$$;
