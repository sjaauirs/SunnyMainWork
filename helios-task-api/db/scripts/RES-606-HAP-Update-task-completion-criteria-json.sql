-- ============================================================================
-- 🚀 Script    : Update_TaskReward_OnSelectionDisplay_For_Multiple_Tenants.sql
-- 📌 Purpose   : Updates task.task_reward.task_completion_criteria_json for multiple tenants
--               based on predefined task_external_code and criteria_json mapping.
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-11-12
-- 🧾 Jira      : RES-606
-- ⚠️ Inputs    :
--               - v_env            → One of DEV / QA / UAT / INTEG / PROD
--               - v_tenant_codes   → Array of tenant codes
-- 📤 Output    : Updates only matching records where delete_nbr = 0
-- 🔗 Script URL: NA
-- 📝 Notes     :
--               - Idempotent: re-running does not duplicate data
--               - Only modifies modalImageUrl and onSelectionDisplay for specified tasks
-- ============================================================================

DO $$
DECLARE
	--  Replace with actual tenant codes	
	v_tenant_codes TEXT[] := ARRAY[
        '<HAP-TENANT-CODE>',
        '<HAP-TENANT-CODE>'
    ];  
    v_env TEXT := 'DEV';  -- Replace with DEV / QA / UAT / INTEG / PROD
	
    v_env_specific_url TEXT;
    v_image_name TEXT := 'niceWork.svg';

    v_mapping JSONB;
    rec JSONB;
    v_task_code TEXT;
    v_criteria_json JSONB;
    v_tenant TEXT;
BEGIN
    -- 🌐 Determine environment-specific base URL
    CASE v_env
        WHEN 'DEV'   THEN v_env_specific_url := 'https://app-static.dev.sunnyrewards.com/cms/images';
        WHEN 'QA'    THEN v_env_specific_url := 'https://app-static.qa.sunnyrewards.com/cms/images';
        WHEN 'UAT'   THEN v_env_specific_url := 'https://app-static.uat.sunnyrewards.com/cms/images';
        WHEN 'INTEG' THEN v_env_specific_url := 'https://app-static.integ.sunnyrewards.com/cms/images';
        WHEN 'PROD'  THEN v_env_specific_url := 'https://app-static.sunnyrewards.com/cms/images';
        ELSE
            RAISE EXCEPTION '❌ Invalid environment [%]. Please choose from DEV, QA, UAT, INTEG, PROD.', v_env;
    END CASE;

    -- Define mapping JSON (contains one or more tasks)
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
							"text": "Systolic less than 120 mmHg AND diastolic less than 80 mmHg"
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
							"text": "You just hit a healthy range and completed an action toward earning a reward. Keep it up — your health is winning!"
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
							"text": "Systolic 120-129 mmHg AND diastolic less than 80 mmHg"
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
							"text": "Systolic 130-139 mmHg OR diastolic 80-89 mmHg"
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
							"text": "Systolic >= 140 mmHg OR diastolic >=90 mmHg"
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
    }
  ]';

    -- 🧩 Loop through each tenant in input array
    FOREACH v_tenant IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE 'Processing tenant: %', v_tenant;

        -- Build env + tenant-specific image URL
        DECLARE
            v_final_url TEXT := v_env_specific_url || '/' || v_tenant || '/' || v_image_name;
        BEGIN
            -- Loop through each task mapping
            FOR rec IN SELECT * FROM jsonb_array_elements(v_mapping)
            LOOP
                v_task_code := rec->>'task_external_code';
                v_criteria_json := rec->'criteria_json';

                -- Replace ENV_SPECIFIC_URL in JSON if found
                IF v_criteria_json #>> '{healthCriteria,uiComponent,0,options,0,modalImageUrl}' IS NOT NULL THEN
                    v_criteria_json := jsonb_set(
                        v_criteria_json,
                        '{healthCriteria,uiComponent,0,options,0,modalImageUrl}',
                        to_jsonb(
                            replace(
                                v_criteria_json #>> '{healthCriteria,uiComponent,0,options,0,modalImageUrl}',
                                'ENV_SPECIFIC_URL',
                                v_final_url
                            )
                        ),
                        true
                    );
                END IF;

                -- Perform the update for this tenant + task
                UPDATE task.task_reward
                SET task_completion_criteria_json = v_criteria_json
                WHERE task_external_code = v_task_code
                  AND tenant_code = v_tenant
                  AND delete_nbr = 0;

                IF FOUND THEN
                    RAISE NOTICE '✅ Updated task [%] for tenant [%]', v_task_code, v_tenant;
                ELSE
                    RAISE NOTICE '⚠️ Task [%] not found for tenant [%]', v_task_code, v_tenant;
                END IF;
            END LOOP;
        END;
    END LOOP;

    RAISE NOTICE '🎯 Script execution completed for all tenants.';
END $$;
