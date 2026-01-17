DO $$
DECLARE
    v_env TEXT := 'DEV'; -- Replace with DEV / QA / UAT / INTEG / PROD
    v_env_specific_url TEXT;
    v_tenant_code TEXT := '<tenant_code_hap>'; -- Replace with actual tenant code
    v_image_name TEXT := 'niceWork.svg';   -- Replace with actual image name

    v_mapping JSONB;
    rec JSONB;
    v_task_code TEXT;
    v_criteria_json JSONB;
BEGIN
    -- Env-specific URL base
    CASE v_env
        WHEN 'DEV'   THEN v_env_specific_url := 'https://app-static.dev.sunnyrewards.com/cms/images';
        WHEN 'QA'    THEN v_env_specific_url := 'https://app-static.qa.sunnyrewards.com/cms/images';
        WHEN 'UAT'   THEN v_env_specific_url := 'https://app-static.uat.sunnyrewards.com/cms/images';
        WHEN 'INTEG' THEN v_env_specific_url := 'https://app-static.integ.sunnyrewards.com/cms/images';
        WHEN 'PROD'  THEN v_env_specific_url := 'https://app-static.sunnyrewards.com/cms/images';
        ELSE
            RAISE EXCEPTION 'Invalid environment [%]. Please choose from DEV, QA, UAT, INTEG, PROD.', v_env;
    END CASE;
	
	    v_env_specific_url := v_env_specific_url || '/' || v_tenant_code || '/' || v_image_name;


    -- ✅ Full v_mapping JSON
    v_mapping := jsonb '[
  {
    "task_external_code": "main_a_heal_bloo_pres",
    "criteria_json": {
      "completionCriteriaType": "HEALTH",
      "completionPeriodType": "QUARTERLY",
      "selfReportType": "UI_COMPONENT",
      "healthCriteria": {
        "healthTaskType": "OTHER",

        "unitType": "reading",
        "unitLabel": { "en-US": "reading" },
        "buttonLabel": { "en-US": "Add Reading" },
        "requiredUnits": 3,

        "uiComponent": [
          {
            "selfReportType": "DROPDOWN",
            "placeholder": { "en-US": "Select" },
            "reportTypeLabel": { "en-US": "Blood Pressure" },
            "isRequiredField": true,
            "multiSelect": false,
            "options": [
              {
                "value": "normal",
                "label": {
				"en-US": [
                    { "type": "header", "data": { "text": "Normal" } },
                    {
                      "type": "paragraph",
                      "data": {
                        "text": "Systolic less than 120 mmHg AND Diastolic less than 80 mmHg"
                      }
                    }
                  ]
                },
                "onSelectionDisplay": {
                  "en-US": [
                    { "type": "header", "data": { "text": "Nice Work!" } },
                    {
                      "type": "paragraph",
                      "data": {
                        "text": "You just hit a healthy range and earned a reward. Keep it up—your health is winning!"
                      }
                    }
                  ]
                },
                "type": "modal",
                "modalImageUrl": "ENV_SPECIFIC_URL"
              },
              {
                "value": "elevated",
                "label": {
				"en-US": [
                    { "type": "header", "data": { "text": "Elevated" } },
                    {
                      "type": "paragraph",
                      "data": {
                        "text": "Systolic 120-129 mmHg AND Diastolic less than 80 mmHg"
                      }
                    }
                  ]
                },
                "onSelectionDisplay": { "en-US": null },
                "type": null
              },
              {
                "value": "high/stage_1",
                "label": {
				"en-US": [
                    { "type": "header", "data": { "text": "High/Stage 1" } },
                    {
                      "type": "paragraph",
                      "data": {
                        "text": "Systolic 130-139 mmHg OR Diastolic 80-89 mmHg"
                      }
                    }
                  ]
                },
                "onSelectionDisplay": {
                  "en-US": "If you take your blood pressure and it is 180/120, wait at least one minute and take it again. If your blood pressure is still high, contact your doctor immediately."
                },
                "type": "option_description"
              },
              {
                "value": "high/stage_2",
                "label": {
				"en-US": [
                    { "type": "header", "data": { "text": "High/Stage 2" } },
                    {
                      "type": "paragraph",
                      "data": {
                        "text": "Systolic >= 140 mmHg OR Diastolic >=90 mmHg"
                      }
                    }
                  ]
                },
                "onSelectionDisplay": {
                  "en-US": "If you take your blood pressure and it is 180/120, wait at least one minute and take it again. If your blood pressure is still high, contact your doctor immediately."
                },
                "type": "option_description"
              }
            ]
          },
          {
            "selfReportType": "DROPDOWN",
            "reportTypeLabel": { "en-US": "Location" },
            "isRequiredField": false,
            "placeholder": { "en-US": "Select" },
            "options": [
              {
                "value": "Home",
                "label": { "en-US": "Home" },
                "onSelectionDisplay": { "en-US": null },
                "type": null
              },
              {
                "value": "My doctor’s office",
                "label": { "en-US": "My doctor’s office" },
                "onSelectionDisplay": { "en-US": null },
                "type": null
              },
              {
                "value": "A local pharmacy",
                "label": { "en-US": "A local pharmacy" },
                "onSelectionDisplay": { "en-US": null },
                "type": null
              },
              {
                "value": "Other location",
                "label": { "en-US": "Other location" },
                "onSelectionDisplay": { "en-US": null },
                "type": null
              }
            ],
            "multiSelect": false
          }
        ],
        "isDialerRequired": true,
        "isDisclaimerAutoChecked": false,
        "skipDisclaimer": true
      }
    }
  },
  {
    "task_external_code": "comp_your_a1c_test",
    "criteria_json": {
      "completionCriteriaType": "HEALTH",
      "completionPeriodType": "MONTH",
      "selfReportType": "UI_COMPONENT",
      "healthCriteria": {
        "healthTaskType": "OTHER",

        "unitType": "reading",
        "unitLabel": { "en-US": "reading" },
        "buttonLabel": { "en-US": "Add Reading" },
        "requiredUnits": 1,

        "uiComponent": [
          {
            "placeholder": { "en-US": "Select" },
            "selfReportType": "DROPDOWN",
            "reportTypeLabel": { "en-US": "Your A1c Reading" },
            "isRequiredField": true,
            "options": [
              {
                "value": "7.0% or less",
                "label": { "en-US": "7.0% or less" },
                "onSelectionDisplay": {
                  "en-US": [
                    { "type": "header", "data": { "text": "Nice Work!" } },
                    {
                      "type": "paragraph",
                      "data": {
                        "text": "You just hit a healthy range and earned a reward. Keep it up—your health is winning!"
                      }
                    }
                  ]
                },
                "type": "modal",
                "modalImageUrl": "ENV_SPECIFIC_URL"
              },
              {
                "value": "7.1% to 9.0%",
                "label": { "en-US": "7.1% to 9.0%" },
                "onSelectionDisplay": { "en-US": null },
                "type": null
              },
              {
                "value": "9.1% or higher",
                "label": { "en-US": "9.1% or higher" },
                "onSelectionDisplay": { "en-US": null },
                "type": null
              }
            ],
            "multiSelect": false
          }
        ],
        "isDialerRequired": false,
        "isDisclaimerAutoChecked": true,
        "skipDisclaimer": false
      }
    }
  },
  {
    "task_external_code": "comp_your_diab_eye_exam",
    "criteria_json": {
      "completionCriteriaType": "HEALTH",
      "completionPeriodType": "MONTH",
      "selfReportType": "UI_COMPONENT",
      "healthCriteria": {
        "healthTaskType": "OTHER",

        "unitType": "Location",
        "unitLabel": { "en-US": "Location" },
        "buttonLabel": { "en-US": "Mark Complete" },
        "requiredUnits": 1,

        "uiComponent": [
          {
            "reportTypeLabel": { "en-US": "Location" },
            "isRequiredField": false,
            "placeholder": { "en-US": "Enter Location" },
            "maxLength": 500
          }
        ],
        "isDialerRequired": false,
        "isDisclaimerAutoChecked": true,
        "skipDisclaimer": false
      }
    }
  },
  {
    "task_external_code": "comp_a_reco_colo_scre",
    "criteria_json": {
      "completionCriteriaType": "HEALTH",
      "completionPeriodType": "MONTH",
      "selfReportType": "UI_COMPONENT",
      "healthCriteria": {
        "healthTaskType": "OTHER",

        "unitType": "Location",
        "unitLabel": { "en-US": "Location" },
        "buttonLabel": { "en-US": "Mark Complete" },
        "requiredUnits": 1,

        "uiComponent": [
          {
            "selfReportType": "TEXTBOX",
            "reportTypeLabel": { "en-US": "Location" },
            "isRequiredField": false,
            "placeholder": { "en-US": "Enter Location" },
            "maxLength": 500
          }
        ],
        "isDialerRequired": false,
        "isDisclaimerAutoChecked": true,
        "skipDisclaimer": false
      }
    }
  },
  {
    "task_external_code": "comp_your_brea_canc_scre",
    "criteria_json": {
      "completionCriteriaType": "HEALTH",
      "completionPeriodType": "MONTH",
      "selfReportType": "UI_COMPONENT",
      "healthCriteria": {
        "healthTaskType": "OTHER",

        "unitType": "Location",
        "unitLabel": { "en-US": "Location" },
        "buttonLabel": { "en-US": "Mark Complete" },
        "requiredUnits": 1,

        "uiComponent": [
          {
            "selfReportType": "TEXTBOX",
            "reportTypeLabel": { "en-US": "Location" },
            "isRequiredField": false,
            "placeholder": { "en-US": "Enter Location" },
            "maxLength": 500
          }
        ],
        "isDialerRequired": false,
        "isDisclaimerAutoChecked": true,
        "skipDisclaimer": false
      }
    }
  },
  {
    "task_external_code": "get_your_flu_vacc",
    "criteria_json": {
      "completionCriteriaType": "HEALTH",
      "completionPeriodType": "MONTH",
      "selfReportType": "UI_COMPONENT",
      "healthCriteria": {
        "healthTaskType": "OTHER",

        "unitType": "Location",
        "unitLabel": { "en-US": "Location" },
        "buttonLabel": { "en-US": "Mark Complete" },
        "requiredUnits": 1,

        "uiComponent": [
          {
            "reportTypeLabel": { "en-US": "Location" },
            "isRequiredField": false,
            "placeholder": { "en-US": "Enter Location" },
            "maxLength": 500,
            "selfReportType": "TEXTBOX"
          }
        ],
        "isDialerRequired": false,
        "isDisclaimerAutoChecked": true,
        "skipDisclaimer": false
      }
    }
  },
  {
    "task_external_code": "conn_with_your_navi",
    "criteria_json": {
      "completionCriteriaType": "HEALTH",
      "completionPeriodType": "MONTH",
      "selfReportType": "UI_COMPONENT",
      "healthCriteria": {
        "healthTaskType": "OTHER",

        "unitType": "Name",
        "unitLabel": { "en-US": "Name" },
        "buttonLabel": { "en-US": "Mark Complete" },
        "requiredUnits": 1,

        "uiComponent": [
          {
            "reportTypeLabel": { "en-US": "Your HAP Navigator''s Name" },
            "isRequiredField": false,
            "selfReportType": "TEXTBOX",
            "placeholder": { "en-US": "Add Name" },
            "maxLength": 500
          }
        ],
        "isDialerRequired": false,
        "isDisclaimerAutoChecked": true,
        "skipDisclaimer": false
      }
    }
  }
]


';

    -- Loop through mapping and update
    FOR rec IN SELECT * FROM jsonb_array_elements(v_mapping)
    LOOP
        v_task_code := rec->>'task_external_code';
        v_criteria_json := rec->'criteria_json';

        -- Replace ENV_SPECIFIC_URL in modalImageUrl if present
        IF v_criteria_json #>> '{healthCriteria,uiComponent,0,options,0,modalImageUrl}' IS NOT NULL THEN
    v_criteria_json := jsonb_set(
        v_criteria_json,
        '{healthCriteria,uiComponent,0,options,0,modalImageUrl}',
        to_jsonb(
            replace(
                v_criteria_json #>> '{healthCriteria,uiComponent,0,options,0,modalImageUrl}',
                'ENV_SPECIFIC_URL',
                v_env_specific_url
            )
        ),
        true
    );
END IF;

        UPDATE task.task_reward
        SET task_completion_criteria_json = v_criteria_json
        WHERE task_external_code = v_task_code
          AND tenant_code = v_tenant_code
          AND delete_nbr = 0;

        IF FOUND THEN
            RAISE NOTICE 'Updated task % successfully', v_task_code;
        ELSE
            RAISE NOTICE 'Task % not found', v_task_code;
        END IF;
    END LOOP;
END $$;