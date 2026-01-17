-- =================================================================================================================================
-- 🚀 Script    : Update Task Reward -> Task Completion Criteria -> selfReportType -> for Rethink your drink to INTERACTIVE
-- 📌 Purpose   : To Task Reward -> Task Completion Criteria -> selfReportType -> for Rethink your drink to INTERACTIVE
-- 🧑 Author    : Neel
-- 📅 Date      : 2025-10-28
-- 🧾 Jira      : SUN-803
-- ⚠️ Inputs    : TENANT_CODE
-- 📤 Output    : Updated tenant_option_json for given tenant(s)
-- 🔗 Script URL: Internal configuration update (QA only)
-- 📝 Notes     : 
--   - JSON key path: benefitsOptions → cardIssueFlowType
--   - Safe to re-run (idempotent)
-- =================================================================================================================================
DO $$
DECLARE

    v_tenant_codes TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_language_code TEXT := 'en-US';
    v_task_headers TEXT[] := ARRAY[
        'Rethink your drink',
        'Rethink your drink 2026'
    ];

    v_task_id BIGINT;
    v_updated_count INTEGER := 0;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        FOR i IN 1..array_length(v_task_headers, 1) LOOP
            -- Fetch the task_id
            SELECT d.task_id INTO v_task_id
            FROM task.task_detail d
            WHERE d.task_header = v_task_headers[i]
            AND d.tenant_code = v_tenant_code
            AND d.language_code = v_language_code
            AND d.delete_nbr = 0;

            IF v_task_id IS NOT NULL THEN

                UPDATE  task.task_reward
                SET task_completion_criteria_json = jsonb_set(
                    task_completion_criteria_json,
                    '{selfReportType}',
                    '"INTERACTIVE"',
                    false
                )
                WHERE task_completion_criteria_json ->> 'selfReportType' = 'INPUT'
                AND task_id = v_task_id
                AND tenant_code = v_tenant_code
                AND delete_nbr = 0;

                RAISE NOTICE 'Updated task reward "%"', v_task_headers[i];
            ELSE
                RAISE NOTICE 'Task not found: %', v_task_headers[i];
            END IF;
        END LOOP;

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;

        IF v_updated_count > 0 THEN
            RAISE NOTICE '✅ Updated task completion criteria for tenant %', v_tenant_code;
        ELSE
            RAISE NOTICE '⚠️ No matching record found or already updated for tenant %', v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE 'Update process completed for all tenants.';
END $$;