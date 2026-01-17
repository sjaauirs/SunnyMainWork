DO $$
DECLARE
    -- 👇 Replace 'KP-TENANT-CODE' with the actual tenant code
    v_tenant_code TEXT := '<KP-TENANT-CODE>';

    v_data JSONB := '[
	  {
		"taskExternalCode": "get_movi_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Minutes",
			"unitLabel": {
			  "es": "minutos",
			  "en-US": "minutes"
			},
			"requiredUnits": 600,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INPUT",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "medi_to_boos_your_well_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Minutes",
			"unitLabel": {
			  "es": "minutos",
			  "en-US": "minutes"
			},
			"requiredUnits": 150,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INPUT",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "stre_your_body_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Days",
			"unitLabel": {
			  "es": "sessions",
			  "en-US": "sessions"
			},
			"requiredUnits": 8,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INTERACTIVE",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "eat_more_seed_and_nuts_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Days",
			"unitLabel": {
			  "es": "días",
			  "en-US": "days"
			},
			"buttonLabel": {
			  "es": "Ahorrar",
			  "en-US": "Save"
			},
			"requiredUnits": 20,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INTERACTIVE",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "eat_the_rain_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Days",
			"unitLabel": {
			  "es": "días",
			  "en-US": "days"
			},
			"buttonLabel": {
			  "es": "Ahorrar",
			  "en-US": "Save"
			},
			"requiredUnits": 8,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INTERACTIVE",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "take_a_brea_from_alco_in_janu_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Days",
			"unitLabel": {
			  "es": "días",
			  "en-US": "days"
			},
			"requiredUnits": 31,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INTERACTIVE",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "take_a_brea_from_alco_in_febr_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Days",
			"unitLabel": {
			  "es": "días",
			  "en-US": "days"
			},
			"requiredUnits": 31,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INTERACTIVE",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "take_a_brea_from_alco_in_marc_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Days",
			"unitLabel": {
			  "es": "días",
			  "en-US": "days"
			},
			"requiredUnits": 31,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INTERACTIVE",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "take_a_stro_afte_a_meal_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Days",
			"unitLabel": {
			  "es": "días",
			  "en-US": "days"
			},
			"buttonLabel": {
			  "es": "Ahorrar",
			  "en-US": "Save"
			},
			"requiredUnits": 20,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INTERACTIVE",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "reth_your_drin_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Days",
			"unitLabel": {
			  "es": "días",
			  "en-US": "days"
			},
			"requiredUnits": 20,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INTERACTIVE",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "step_it_up_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Steps",
			"unitLabel": {
			  "es": "pasos",
			  "en-US": "steps"
			},
			"requiredSteps": 200000,
			"healthTaskType": "STEPS"
		  },
		  "selfReportType": "INPUT",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "conn_with_thos_who_make_you_smil_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Days",
			"unitLabel": {
			  "es": "días",
			  "en-US": "days"
			},
			"buttonLabel": {
			  "es": "Ahorrar",
			  "en-US": "Save"
			},
			"requiredUnits": 4,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INTERACTIVE",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "get_your_z_s_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Nights",
			"unitLabel": {
			  "es": "noches",
			  "en-US": "nights"
			},
			"requiredUnits": 24,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INPUT",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "live_a_life_of_grat_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Entries",
			"unitLabel": {
			  "es": "anotaciones en el diario",
			  "en-US": "journal entries"
			},
			"buttonLabel": {
			  "es": "Ahorrar",
			  "en-US": "Save"
			},
			"requiredUnits": 24,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INTERACTIVE",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "powe_down_befo_bed_2026",
		"taskCompletionCriteriaJson": {
		  "healthCriteria": {
			"unitType": "Nights",
			"unitLabel": {
			  "es": "noches",
			  "en-US": "nights"
			},
			"buttonLabel": {
			  "es": "Ahorrar",
			  "en-US": "Save"
			},
			"requiredUnits": 20,
			"healthTaskType": "OTHER"
		  },
		  "selfReportType": "INTERACTIVE",
		  "completionPeriodType": "MONTH",
		  "completionCriteriaType": "HEALTH"
		}
	  },
	  {
		"taskExternalCode": "volu_your_time_2026",
		"taskCompletionCriteriaJson": {
          "imageCriteria": {
            "unitLabel": {
              "es": "fotos",
              "en-US": "photos"
            },
            "buttonLabel": {
              "es": "Añadir foto",
              "en-US": "Add photo"
            },
            "requiredImageCount": 1,
            "icon": { "modalIconUrl": null },
            "imageCriteriaText": {
              "en-US": [
                { "type": "header", "data": { "text": "Show how you give back" }},
                { "type": "paragraph", "data": { "text": "Upload a photo of your volunteer work this month." }}
              ],
               "es": [
                { "type": "header", "data": { "text": "Muestre su contribución" }},
                { "type": "paragraph", "data": { "text": "Suba una foto de su voluntariado de este mes." }}
              ]
            },
            "imageCriteriaTextAlignment": "left"
          },
          "completionCriteriaType": "IMAGE"
        }
	  }
	]';

    task_record JSONB;
    v_task_code TEXT;
    v_task_json JSONB;
    v_updated_count INT;
BEGIN
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

    RAISE NOTICE '🎉 Task update process complete!';
END $$;