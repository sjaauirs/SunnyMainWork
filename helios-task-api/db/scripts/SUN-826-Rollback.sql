-- =================================================================================================================================
-- 🚀 Script    : Rollback - Update task_completion_criteria_json to NULL for get_your_flu_vacc
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
    v_json JSONB := '{
        "healthCriteria": {
            "unitType": "reading",
            "unitLabel": {"en-US": "reading"},
            "buttonLabel": {"en-US": "Add reading"},
            "uiComponent": [
                {
                    "options": [
                        {
                            "type": "modal",
                            "label": {"en-US": "7.0% or less"},
                            "value": "7.0% or less",
                            "modalImageUrl": "https://app-static.qa.sunnyrewards.com/cms/images/ten-b4e920d3f6f74496ab533d1a9a8ef9e4/niceWork.svg",
                            "onSelectionDisplay": {
                                "en-US": [
                                    {"data": {"text": "Nice work!"}, "type": "header"},
                                    {"data": {"text": "You just hit a healthy range and earned a reward. Keep it up—your health is winning!"}, "type": "paragraph"}
                                ]
                            }
                        },
                        {
                            "type": null,
                            "label": {"en-US": "7.1% to 9.0%"},
                            "value": "7.1% to 9.0%",
                            "onSelectionDisplay": {"en-US": null}
                        },
                        {
                            "type": null,
                            "label": {"en-US": "9.1% or higher"},
                            "value": "9.1% or higher",
                            "onSelectionDisplay": {"en-US": null}
                        }
                    ],
                    "multiSelect": false,
                    "placeholder": {"en-US": "Select"},
                    "selfReportType": "DROPDOWN",
                    "isRequiredField": true,
                    "reportTypeLabel": {"en-US": "Your A1C Reading"}
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
    }'::jsonb;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE task.task_reward
        SET task_completion_criteria_json = v_json,
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND task_external_code = 'get_your_flu_vacc'
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        RAISE NOTICE '♻️ Rollback executed for tenant % — % row(s) restored.', v_tenant_code, v_rowcount;
    END LOOP;

    RAISE NOTICE '✅ Rollback completed for % tenant(s).', array_length(v_tenant_codes, 1);
END $$;