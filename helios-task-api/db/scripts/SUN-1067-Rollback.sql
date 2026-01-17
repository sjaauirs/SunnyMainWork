-- =================================================================================================================================
-- 🚀 Script    : Rever Task Reward -> Task Completion Criteria -> max trackable days to 24 for Rethink your drink
-- 📌 Purpose   : To Task Reward -> Task Completion Criteria -> selfReportType -> max trackable days to 24 for Rethink your drink 
-- 🧑 Author    : Neel
-- 📅 Date      : 2025-11-07
-- 🧾 Jira      : SUN-1067
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
    v_task_id BIGINT;
    v_updated_count INTEGER := 0;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP            
		UPDATE task.task_reward
		SET task_completion_criteria_json = jsonb_set(
			task_completion_criteria_json,
			'{healthCriteria,requiredUnits}',
			'20'::jsonb,
			false
		)
		WHERE tenant_code = v_tenant_code
		  AND task_external_code = 'reth_your_drin_2026';

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;

        IF v_updated_count > 0 THEN
            RAISE NOTICE '✅ Updated task completion criteria for tenant %', v_tenant_code;
        ELSE
            RAISE NOTICE '⚠️ No matching record found or already updated for tenant %', v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE 'Update process completed for all tenants.';
END $$;