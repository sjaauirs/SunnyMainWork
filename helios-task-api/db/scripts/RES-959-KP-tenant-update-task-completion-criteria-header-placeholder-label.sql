-- ============================================================================
-- 🚀 Script    : Update Task Reward Labels for Health Criteria
-- 📌 Purpose   : Add or update `headerLabel` and `placeHolderLabel` fields under 
--                `healthCriteria` in `task.task_reward.task_completion_criteria_json`
--                for the given tenant and task external codes.
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-10-24
-- 🧾 Jira      : RES-959
-- ⚠️ Inputs    : KP-TENANT-CODE
-- 📤 Output    : Updates `task_completion_criteria_json` with multilingual labels.
-- 🔗 Script URL: NA
-- 📝 Notes     : 
--                - The script is idempotent (safe to re-run; it overwrites existing values).
--                - Uses jsonb_set to add/replace JSON keys under healthCriteria.
-- ============================================================================

DO $$
DECLARE
    tenantCode TEXT := '<KP-TENANT-CODE>'; -- Input tenant code

    inputJson JSON := '[
    {
        "taskExternalCode": "get_your_z_s",
        "taskExternalCode_2026": "get_your_z_s_2026",
		"headerLabel": {
			"es": "Empezar a hacer un seguimiento de su sueño",
			"en-US": "Start tracking your sleep"
		},
		"placeHolderLabel": {
			"es": "Añada sus noches de sueño (ej. 1)",
			"en-US": "Add your nights of sleep (ex.1)"
		}	
    },
	{
        "taskExternalCode": "get_movi_2026",
		"headerLabel": {
			"es": "Empiece a registrar sus minutos activos",
			"en-US": "Start tracking your active minutes"
		},
		"placeHolderLabel": {
			"es": "Añada sus minutos activos (ej. 30)",
			"en-US": "Add your active minutes (ex.30)"
		}	
    },
	{
        "taskExternalCode": "stre_your_body",
		"taskExternalCode_2026": "stre_your_body_2026",
		"headerLabel": {
			"es": "Empiece a registrar su entrenamiento",
			"en-US": "Start tracking your workouts"
		},
		"placeHolderLabel": {
			"es": "Añada sus sesiones de entrenamiento (ej. 1)",
			"en-US": "Add your workout sessions (ex.1)"
		}	
    },
	{
        "taskExternalCode": "step_it_up",
		"taskExternalCode_2026": "step_it_up_2026",
		"headerLabel": {
			"es": "Empiece a registrar sus paso",
			"en-US": "Start tracking your steps"
		},
		"placeHolderLabel": {
			"es": "Añada sus pasos (ej. 5,000)",
			"en-US": "Add your steps (ex.5,000)"
		}	
    },
	{
        "taskExternalCode": "medi_to_boos_your_well",
		"taskExternalCode_2026": "medi_to_boos_your_well_2026",
		"headerLabel": {
			"es": "Empiece a registrar sus minutos",
			"en-US": "Start tracking your minutes"
		},
		"placeHolderLabel": {
			"es": "Añada sus minutos (ej. 30 minutos)",
			"en-US": "Add your minutes (ex.30 minutes)"
		}	
    }
]'; 

    item JSON;
    taskCode TEXT;
    taskCode2026 TEXT;
    headerLbl JSON;
    placeHolderLbl JSON;
    updatedCount INT;
    codeToUpdate TEXT;
BEGIN
    RAISE NOTICE 'Starting task reward label update for tenant: %', tenantCode;

    -- Loop through each element of the input JSON array
    FOR item IN SELECT * FROM json_array_elements(inputJson)
    LOOP
        taskCode := item->>'taskExternalCode';
        taskCode2026 := item->>'taskExternalCode_2026';
        headerLbl := item->'headerLabel';
        placeHolderLbl := item->'placeHolderLabel';

        -- Process both taskExternalCodes (if present)
        FOR codeToUpdate IN 
            SELECT x FROM unnest(ARRAY[taskCode, taskCode2026]) AS x
            WHERE x IS NOT NULL
        LOOP
            UPDATE task.task_reward
            SET task_completion_criteria_json = jsonb_set(
                jsonb_set(
                    COALESCE(task_completion_criteria_json, '{}'::jsonb),
                    '{healthCriteria,headerLabel}', 
                    headerLbl::jsonb, 
                    TRUE
                ),
                '{healthCriteria,placeHolderLabel}', 
                placeHolderLbl::jsonb, 
                TRUE
            )
            WHERE tenant_code = tenantCode
              AND task_external_code = codeToUpdate
              AND delete_nbr = 0;

            GET DIAGNOSTICS updatedCount = ROW_COUNT;

            IF updatedCount > 0 THEN
                RAISE NOTICE '✅ Updated task_external_code=% with new labels for tenant=%', 
                    codeToUpdate, tenantCode;
            ELSE
                RAISE NOTICE '⚠️ No record found for task_external_code=% (tenant=%)', 
                    codeToUpdate, tenantCode;
            END IF;
        END LOOP;
    END LOOP;

    RAISE NOTICE '🎯 Label update process completed for tenant: %', tenantCode;
END $$;