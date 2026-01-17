-- =================================================================================================================================
-- 🚀 Script    : Update task_completion_criteria_json to accept location for get_your_flu_vacc
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-10-23
-- 🧾 Jira      : SUN-826
-- ⚠️ Inputs    : HAP_TENANT_CODE
-- =================================================================================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'HAP-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_rowcount INT := 0;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE task.task_reward
        SET task_completion_criteria_json = '{
            "healthCriteria": {
                "unitType": "Location",
                "unitLabel": {"en-US": "Location"},
                "buttonLabel": {"en-US": "Mark complete"},
                "uiComponent": [
                    {
                        "maxLength": 500,
                        "placeholder": {"en-US": "Enter Location"},
                        "selfReportType": "TEXTBOX",
                        "isRequiredField": false,
                        "reportTypeLabel": {"en-US": "Location"}
                    }
                ],
                "requiredUnits": 1,
                "healthTaskType": "OTHER",
                "skipDisclaimer": false,
                "isDialerRequired": false,
                "isDisclaimerAutoChecked": true
            },
            "selfReportType": "UI_COMPONENT",
            "completionPeriodType": "MONTH",
            "completionCriteriaType": "HEALTH"
        }'::jsonb,
        update_ts = NOW(),
        update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND task_external_code = 'get_your_flu_vacc'
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        RAISE NOTICE '✅ Tenant % → % rows updated', v_tenant_code, v_rowcount;
    END LOOP;
END $$;