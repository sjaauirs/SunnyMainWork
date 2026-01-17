-- ==================================================================================================
-- 🚀 Script    : Rever task_completion_criteria_json for sleep, workout and mindful activities
-- 🧑 Author    : Neel Kunchakurti
-- 📅 Date      : 2025-11-05
-- 🧾 Jira      : RES-55
-- ⚠️ Inputs    : 
-- 📤 Output    : updates task.task_reward
-- 🔗 Script URL: 
-- 📝 Notes     : 
-- ==================================================================================================


DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_rowcount INT := 0;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
      UPDATE task.task_reward
        SET task_completion_criteria_json = '{
            "healthCriteria": {
                "unitType": "Nights",
                "unitLabel": {
                "es": "noches",
                "en-US": "nights"
                },
                "headerLabel": {
                "es": "Empezar a hacer un seguimiento de su sueño",
                "en-US": "Start tracking your sleep"
                },
                "requiredSleep": {
                "minSleepDuration": 420,
                "numDaysAtOrAboveMinDuration": 20
                },
                "healthTaskType": "SLEEP",
                "placeHolderLabel": {
                "es": "Añada sus noches de sueño (ej. 1)",
                "en-US": "Add your nights of sleep (ex.1)"
                }
            },
            "selfReportType": "INTERACTIVE",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
            }'::jsonb,
        update_ts = NOW(),
        update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND task_external_code IN ('get_your_z_s_2026', 'get_your_z_s')
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        RAISE NOTICE '✅ Task % → % rows updated', v_tenant_code, v_rowcount;

        UPDATE task.task_reward
        SET task_completion_criteria_json = '
            {
            "healthCriteria": {
                "unitType": "Minutes",
                "unitLabel": {
                "es": "minutos",
                "en-US": "minutes"
                },
                "headerLabel": {
                "es": "Empiece a registrar sus minutos",
                "en-US": "Start tracking your minutes"
                },
                "requiredUnits": 150,
                "healthTaskType": "OTHER",
                "placeHolderLabel": {
                "es": "Añada sus minutos (ej. 30 minutos)",
                "en-US": "Add your minutes (ex.30 minutes)"
                }
            },
            "selfReportType": "INPUT",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
            }'::jsonb,
        update_ts = NOW(),
        update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND task_external_code IN ('medi_to_boos_your_well', 'medi_to_boos_your_well_2026')
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        RAISE NOTICE '✅ Task % → % rows updated', v_tenant_code, v_rowcount;

        
        UPDATE task.task_reward
        SET task_completion_criteria_json = '
            {
                "healthCriteria": {
                    "unitType": "Days",
                    "unitLabel": {
                    "es": "días",
                    "en-US": "days"
                    },
                    "headerLabel": {
                    "es": "Empiece a registrar su entrenamiento",
                    "en-US": "Start tracking your workouts"
                    },
                    "requiredUnits": 8,
                    "healthTaskType": "OTHER",
                    "placeHolderLabel": {
                    "es": "Añada sus sesiones de entrenamiento (ej. 1)",
                    "en-US": "Add your workout sessions (ex.1)"
                    }
                },
                "selfReportType": "INPUT",
                "completionPeriodType": "MONTH",
                "completionCriteriaType": "HEALTH"
                }'::jsonb,
        update_ts = NOW(),
        update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND task_external_code IN ('stre_your_body_2026', 'stre_your_body')
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        RAISE NOTICE '✅ Task % → % rows updated', v_tenant_code, v_rowcount;

    END LOOP;

END
$$;
-- ==================================================================================================