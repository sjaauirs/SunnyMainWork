-- ============================================================================
-- 🚀 Script    : Script to update task_completion_criteria_json
-- 📌 Purpose   : By executing this script database updates to the actual task completionCriteria in task.task_reward
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 07-Oct-2025
-- 🧾 Jira      : RES-738, RES-746, RES-749(Defects)
-- ⚠️ Inputs    : HAP-TENANT-CODE, Environment
-- 📤 Output    : Updates the task completion criteria for HAP-Tenant-Code
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ============================================================================

DO $$
DECLARE
    v_env TEXT := 'DEV'; -- Replace with DEV / QA / UAT / INTEG / PROD
    v_env_specific_url TEXT;
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- Replace with actual tenant code
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
      "healthCriteria": {
        "unitType": "reading",
        "unitLabel": {
          "en-US": "reading"
        },
        "buttonLabel": {
          "en-US": "Add reading"
        },
        "uiComponent": [
          {
            "options": [
              {
                "type": "modal",
                "label": {
                  "en-US": [
                    {
                      "data": {
                        "text": "Normal"
                      },
                      "type": "header"
                    },
                    {
                      "data": {
                        "text": "Systolic less than 120 mmHg AND Diastolic less than 80 mmHg"
                      },
                      "type": "paragraph"
                    }
                  ]
                },
                "value": "normal",
                "modalImageUrl": "ENV_SPECIFIC_URL",
                "onSelectionDisplay": {
                  "en-US": [
                    {
                      "data": {
                        "text": "Nice work!"
                      },
                      "type": "header"
                    },
                    {
                      "data": {
                        "text": "You just hit a healthy range and earned a reward. Keep it up—your health is winning!"
                      },
                      "type": "paragraph"
                    }
                  ]
                }
              },
              {
                "type": null,
                "label": {
                  "en-US": [
                    {
                      "data": {
                        "text": "Elevated"
                      },
                      "type": "header"
                    },
                    {
                      "data": {
                        "text": "Systolic 120-129 mmHg AND Diastolic less than 80 mmHg"
                      },
                      "type": "paragraph"
                    }
                  ]
                },
                "value": "elevated",
                "onSelectionDisplay": {
                  "en-US": null
                }
              },
              {
                "type": "option_description",
                "label": {
                  "en-US": [
                    {
                      "data": {
                        "text": "High/Stage 1"
                      },
                      "type": "header"
                    },
                    {
                      "data": {
                        "text": "Systolic 130-139 mmHg OR Diastolic 80-89 mmHg"
                      },
                      "type": "paragraph"
                    }
                  ]
                },
                "value": "high/stage_1",
                "onSelectionDisplay": {
                  "en-US": "If you take your blood pressure and it is 180/120, wait at least one minute and take it again. If your blood pressure is still high, contact your doctor immediately."
                }
              },
              {
                "type": "option_description",
                "label": {
                  "en-US": [
                    {
                      "data": {
                        "text": "High/Stage 2"
                      },
                      "type": "header"
                    },
                    {
                      "data": {
                        "text": "Systolic >= 140 mmHg OR Diastolic >=90 mmHg"
                      },
                      "type": "paragraph"
                    }
                  ]
                },
                "value": "high/stage_2",
                "onSelectionDisplay": {
                  "en-US": "If you take your blood pressure and it is 180/120, wait at least one minute and take it again. If your blood pressure is still high, contact your doctor immediately."
                }
              }
            ],
            "multiSelect": false,
            "placeholder": {
              "en-US": "Select"
            },
            "selfReportType": "DROPDOWN",
            "isRequiredField": true,
            "reportTypeLabel": {
              "en-US": "Blood Pressure"
            }
          },
          {
            "options": [
              {
                "type": null,
                "label": {
                  "en-US": "Home"
                },
                "value": "Home",
                "onSelectionDisplay": {
                  "en-US": null
                }
              },
              {
                "type": null,
                "label": {
                  "en-US": "My doctor’s office"
                },
                "value": "My doctor’s office",
                "onSelectionDisplay": {
                  "en-US": null
                }
              },
              {
                "type": null,
                "label": {
                  "en-US": "A local pharmacy"
                },
                "value": "A local pharmacy",
                "onSelectionDisplay": {
                  "en-US": null
                }
              },
              {
                "type": null,
                "label": {
                  "en-US": "Other location"
                },
                "value": "Other location",
                "onSelectionDisplay": {
                  "en-US": null
                }
              }
            ],
            "multiSelect": false,
            "placeholder": {
              "en-US": "Select"
            },
            "selfReportType": "DROPDOWN",
            "isRequiredField": false,
            "reportTypeLabel": {
              "en-US": "Location"
            }
          }
        ],
        "requiredUnits": 3,
        "healthTaskType": "OTHER",
        "skipDisclaimer": true,
        "isDialerRequired": true,
        "isDisclaimerAutoChecked": false
      },
      "selfReportType": "UI_COMPONENT",
      "completionPeriodType": "QUARTERLY",
      "completionCriteriaType": "HEALTH"
    }
  },
  {
    "task_external_code": "comp_your_a1c",
    "criteria_json": {
      "healthCriteria": {
        "unitType": "reading",
        "unitLabel": {
          "en-US": "reading"
        },
        "buttonLabel": {
          "en-US": "Add reading"
        },
        "uiComponent": [
          {
            "options": [
              {
                "type": "modal",
                "label": {
                  "en-US": "7.0% or less"
                },
                "value": "7.0% or less",
                "modalImageUrl": "ENV_SPECIFIC_URL",
                "onSelectionDisplay": {
                  "en-US": [
                    {
                      "data": {
                        "text": "Nice work!"
                      },
                      "type": "header"
                    },
                    {
                      "data": {
                        "text": "You just hit a healthy range and earned a reward. Keep it up—your health is winning!"
                      },
                      "type": "paragraph"
                    }
                  ]
                }
              },
              {
                "type": null,
                "label": {
                  "en-US": "7.1% to 9.0%"
                },
                "value": "7.1% to 9.0%",
                "onSelectionDisplay": {
                  "en-US": null
                }
              },
              {
                "type": null,
                "label": {
                  "en-US": "9.1% or higher"
                },
                "value": "9.1% or higher",
                "onSelectionDisplay": {
                  "en-US": null
                }
              }
            ],
            "multiSelect": false,
            "placeholder": {
              "en-US": "Select"
            },
            "selfReportType": "DROPDOWN",
            "isRequiredField": true,
            "reportTypeLabel": {
              "en-US": "Your A1C Reading"
            }
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
    }
  },
  {
    "task_external_code": "comp_your_diab_eye_exam",
    "criteria_json": {
      "healthCriteria": {
        "unitType": "Location",
        "unitLabel": {
          "en-US": "Location"
        },
        "buttonLabel": {
          "en-US": "Mark complete"
        },
        "uiComponent": [
          {
            "maxLength": 500,
            "placeholder": {
              "en-US": "Enter Location"
            },
            "isRequiredField": false,
            "reportTypeLabel": {
              "en-US": "Location"
            }
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
    }
  },
  {
    "task_external_code": "comp_a_reco_colo_scre",
    "criteria_json": {
      "healthCriteria": {
        "unitType": "Location",
        "unitLabel": {
          "en-US": "Location"
        },
        "buttonLabel": {
          "en-US": "Mark complete"
        },
        "uiComponent": [
          {
            "maxLength": 500,
            "placeholder": {
              "en-US": "Enter Location"
            },
            "selfReportType": "TEXTBOX",
            "isRequiredField": false,
            "reportTypeLabel": {
              "en-US": "Location"
            }
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
    }
  },
  {
    "task_external_code": "comp_your_brea_canc_scre",
    "criteria_json": {
      "healthCriteria": {
        "unitType": "Location",
        "unitLabel": {
          "en-US": "Location"
        },
        "buttonLabel": {
          "en-US": "Mark complete"
        },
        "uiComponent": [
          {
            "maxLength": 500,
            "placeholder": {
              "en-US": "Enter Location"
            },
            "selfReportType": "TEXTBOX",
            "isRequiredField": false,
            "reportTypeLabel": {
              "en-US": "Location"
            }
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
        "buttonLabel": { "en-US": "Mark complete" },
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
      "healthCriteria": {
        "unitType": "Name",
        "unitLabel": {
          "en-US": "Name"
        },
        "buttonLabel": {
          "en-US": "Mark Complete"
        },
        "uiComponent": [
          {
            "maxLength": 500,
            "placeholder": {
              "en-US": "Add Name"
            },
            "selfReportType": "TEXTBOX",
            "isRequiredField": false,
            "reportTypeLabel": {
              "en-US": "Your HAP Navigator''s Name"
            }
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
    }
  },
  {
	   "task_external_code": "lear_abou_pres_home_deli",
	   "criteria_json": {
	   "disableTriviaSplashScreen": true
		}
  }
]';

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
