--===============================================================================
-- Script:   Rollback task completion criteria of your voice mater survey action
-- Inputs:   v_tenant_codes       - array of tenant codes
-- Output:   Update rows in task.task_reward; logs progress
-- Notes:    Safe to run multiple times;
-- Author:   Kumar Sirikonda
-- Date:     03-12-2025
--===============================================================================
DO $$
DECLARE
    -- 🏢 Input tenant codes
    v_tenant_codes TEXT[] := ARRAY[
        '<KP-TENANT-CODE>',
        '<KP-TENANT-CODE>'
    ];

    v_data JSONB := '[
	  {
		"taskExternalCode": "your_voic_matt",
		"taskCompletionCriteriaJson": {}
	  }
	]';

    task_record JSONB;
    v_task_code TEXT;
    v_task_json JSONB;
    v_updated_count INT;
    v_tenant_code TEXT;
BEGIN
	 FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        RAISE NOTICE '------------------------------------------------';
        RAISE NOTICE '➡️  Processing tenant: %', v_tenant_code;
		-- Loop through JSON array
		FOR task_record IN
			SELECT * FROM jsonb_array_elements(v_data)
		LOOP
			v_task_code := task_record->>'taskExternalCode';
			v_task_json := task_record->'taskCompletionCriteriaJson';

			-- Update matching tasks
			UPDATE task.task_reward
			SET task_completion_criteria_json = v_task_json,
				update_ts = NOW(),
				update_user = 'SYSTEM'
			WHERE task_external_code = v_task_code
			  AND tenant_code = v_tenant_code
			  AND delete_nbr = 0;

			GET DIAGNOSTICS v_updated_count = ROW_COUNT;

			IF v_updated_count > 0 THEN
				RAISE NOTICE '✅ Updated task: % (tenant: %)', v_task_code, v_tenant_code;
			ELSE
				RAISE NOTICE '⚠️ No matching task found for: % (tenant: %)', v_task_code, v_tenant_code;
			END IF;
		END LOOP;
    END LOOP;

    RAISE NOTICE '🎉 Task update process complete!';
END $$;