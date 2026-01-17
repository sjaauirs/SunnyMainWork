-- ============================================================================
-- 🎯 Purpose   : Revert unitLabel.en-US and buttonLabel.en-US in task_reward
-- 🧾 Jira      : RES-1387
-- 📝 Summary   :
--               • Iterates through tenant_code list
--               • Iterates through task_external_code list
--               • Updates matching task_reward records
--               • Ensures healthCriteria exists in completion_criteria_json
--               • Updates:
--                   - healthCriteria.unitLabel.en-US
--                   - healthCriteria.buttonLabel.en-US
--               • Safe to rerun (idempotent)
--
-- 📌 Parameters:
--      ▪ v_tenant_codes         : Array of tenant codes
--      ▪ v_task_external_codes  : Array of task external codes
--
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        '<HAP-TENANT-CODE-1>',
        '<HAP-TENANT-CODE-2>'
    ];

    v_task_external_codes TEXT[] := ARRAY[
        'main_a_heal_bloo_pres'
    ];

    v_tenant_code TEXT;
    v_task_external_code TEXT;
    v_task_reward_id BIGINT;
    v_json JSONB;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        FOREACH v_task_external_code IN ARRAY v_task_external_codes
        LOOP
            FOR v_task_reward_id, v_json IN
                SELECT task_reward_id, task_completion_criteria_json
                FROM task.task_reward
                WHERE tenant_code = v_tenant_code
                  AND task_external_code = v_task_external_code
				  AND delete_nbr = 0
            LOOP
                IF v_json IS NULL THEN
                    RAISE NOTICE '⚠️ Skipped: completion_criteria_json is NULL (tenant=%, task=%)',
                                 v_tenant_code, v_task_external_code;
                    CONTINUE;
                END IF;

                IF v_json ? 'healthCriteria' THEN
                    v_json :=
                        jsonb_set(
                            jsonb_set(
                                v_json,
                                '{healthCriteria,unitLabel,en-US}',
                                '"readings"'::jsonb,       -- 🔁 NEW unit label
                                true
                            ),
                            '{healthCriteria,buttonLabel,en-US}',
                            '"Add reading"'::jsonb,     -- 🔁 NEW button label
                            true
                        );

                    UPDATE task.task_reward
                    SET task_completion_criteria_json = v_json
                    WHERE task_reward_id = v_task_reward_id
					AND delete_nbr = 0;

                    RAISE NOTICE '✅ Updated labels (tenant=%, task=%)',
                                 v_tenant_code, v_task_external_code;
                ELSE
                    RAISE NOTICE '⚠️ Skipped: healthCriteria missing (tenant=%, task=%)',
                                 v_tenant_code, v_task_external_code;
                END IF;
            END LOOP;
        END LOOP;
    END LOOP;
END $$;
