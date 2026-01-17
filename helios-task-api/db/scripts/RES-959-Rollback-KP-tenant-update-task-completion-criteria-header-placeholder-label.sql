-- ============================================================================
-- 🚀 Script    : Rollback - Remove Health Criteria Labels from Task Reward
-- 📌 Purpose   : Removes `headerLabel` and `placeHolderLabel` under 
--                `healthCriteria` in `task.task_reward.task_completion_criteria_json`
--                for the specified tenant and task external codes.
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-10-24
-- 🧾 Jira      : RES-959
-- ⚠️ Inputs    : KP-TENANT-CODE
-- 📤 Output    : Reverts label updates applied by RES-959 script.
-- 🔗 Script URL: NA
-- 📝 Notes     :
--                - Safely removes only the added keys, preserving all other data.
--                - Idempotent: can be re-run safely without side effects.
-- ============================================================================

DO $$
DECLARE
    tenantCode TEXT := '<KP-TENANT-CODE>'; -- Input tenant code

    inputJson JSON := '[
    {
        "taskExternalCode": "get_your_z_s",
        "taskExternalCode_2026": "get_your_z_s_2026"
    },
	{
        "taskExternalCode": "get_movi_2026"
    },
	{
        "taskExternalCode": "stre_your_body",
        "taskExternalCode_2026": "stre_your_body_2026"
    },
	{
        "taskExternalCode": "step_it_up",
        "taskExternalCode_2026": "step_it_up_2026"
    },
	{
        "taskExternalCode": "medi_to_boos_your_well",
        "taskExternalCode_2026": "medi_to_boos_your_well_2026"
    }
]'; 

    item JSON;
    taskCode TEXT;
    taskCode2026 TEXT;
    codeToUpdate TEXT;
    updatedCount INT;
BEGIN
    RAISE NOTICE '🔄 Starting rollback of task reward label updates for tenant: %', tenantCode;

    -- Loop through each element of the input JSON array
    FOR item IN SELECT * FROM json_array_elements(inputJson)
    LOOP
        taskCode := item->>'taskExternalCode';
        taskCode2026 := item->>'taskExternalCode_2026';

        -- Process both taskExternalCodes (if present)
        FOR codeToUpdate IN 
            SELECT x FROM unnest(ARRAY[taskCode, taskCode2026]) AS x
            WHERE x IS NOT NULL
        LOOP
            UPDATE task.task_reward
            SET task_completion_criteria_json =
                -- Remove both keys from healthCriteria
                jsonb_set(
                    jsonb_set(
                        task_completion_criteria_json #- '{healthCriteria,headerLabel}',
                        '{healthCriteria,placeHolderLabel}',
                        'null'::jsonb,
                        TRUE
                    ),
                    '{healthCriteria}',
                    CASE 
                        WHEN (task_completion_criteria_json->'healthCriteria') IS NULL OR
                             (task_completion_criteria_json->'healthCriteria' = '{}'::jsonb)
                        THEN NULL
                        ELSE (task_completion_criteria_json->'healthCriteria')
                    END,
                    TRUE
                )
            WHERE tenant_code = tenantCode
              AND task_external_code = codeToUpdate
              AND delete_nbr = 0;

            GET DIAGNOSTICS updatedCount = ROW_COUNT;

            IF updatedCount > 0 THEN
                RAISE NOTICE '✅ Rolled back labels for task_external_code=% (tenant=%)', 
                    codeToUpdate, tenantCode;
            ELSE
                RAISE NOTICE '⚠️ No record found or already clean for task_external_code=% (tenant=%)', 
                    codeToUpdate, tenantCode;
            END IF;
        END LOOP;
    END LOOP;

    RAISE NOTICE '🎯 Rollback completed successfully for tenant: %', tenantCode;
END $$;
