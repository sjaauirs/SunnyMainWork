--===============================================================================
-- Script:   inserting new task category 
-- Inputs:   v_tenant_codes       - array of tenant codes
-- Output:   Update rows in task.task_category; logs progress
-- Story:    RES-1106 and RES-124
-- Author:   Kawalpreet Kaur
-- Date:     08-12-2025
--===============================================================================
DO
$$
DECLARE
    -- JSON array input
    v_input JSON := '[
        { "v_task_category_name": "Steps", "v_description": "Health and Wellness Steps" },
        { "v_task_category_name": "Healthy eating", "v_description": "Health and Wellness Healthy eating" },
        { "v_task_category_name": "Trivia", "v_description": "Trivia" },
        { "v_task_category_name": "Wellness", "v_description": "Wellness" },
        { "v_task_category_name": "Strength", "v_description": "Strength" },
        { "v_task_category_name": "Shopping", "v_description": "Shopping" },
        { "v_task_category_name": "Vaccine", "v_description": "Vaccine" },
        { "v_task_category_name": "Work", "v_description": "Work" },
        { "v_task_category_name": "Sleep", "v_description": "Health and Wellness Sleep" },
        { "v_task_category_name": "Enrollment", "v_description": "Enrollment Benefits" },
        { "v_task_category_name": "Card", "v_description": "Card Benefits" },
        { "v_task_category_name": "Health", "v_description": "Health Benefits" },
        { "v_task_category_name": "Beneficiary", "v_description": "Beneficiary Benefits" },
        { "v_task_category_name": "Positivity",   "v_description": "Positivity Benefits" }
    ]';

    v_item JSON;
    v_task_category_name TEXT;
    v_description TEXT;
    v_task_category_code TEXT;
    v_exists BOOLEAN;

BEGIN
    -- Loop through each JSON object
    FOR v_item IN SELECT * FROM json_array_elements(v_input)
    LOOP
        -- Extract values
        v_task_category_name := v_item->>'v_task_category_name';
        v_description        := v_item->>'v_description';

        -- Generate code using MD5 hash
        v_task_category_code := 'tcc-' || md5(v_task_category_name);

        -- Check if record exists
        SELECT EXISTS (
            SELECT 1
            FROM task.task_category
            WHERE task_category_code = v_task_category_code
              AND task_category_name = v_task_category_name
              AND delete_nbr = 0
        )
        INTO v_exists;

        IF v_exists THEN
            -- Update existing record
            UPDATE task.task_category
            SET task_category_description = v_description,
                update_user = 'SYSTEM',
                update_ts = NOW()
            WHERE task_category_code = v_task_category_code
              AND task_category_name = v_task_category_name
              AND delete_nbr = 0;

            RAISE NOTICE '✅ Updated: %', v_task_category_name;
        ELSE
            -- Insert new record
            INSERT INTO task.task_category (
                task_category_code,
                task_category_description,
                create_ts,
                update_ts,
                create_user,
                update_user,
                delete_nbr,
                task_category_name
            )
            VALUES (
                v_task_category_code,
                v_description,
                NOW(),
                NULL,
                'SYSTEM',
                NULL,
                0,
                v_task_category_name
            );

            RAISE NOTICE '🆕 Inserted: %', v_task_category_name;
        END IF;

    END LOOP;
END
$$;

--===============================================================================
-- Script:   inserting new tenant_task_category action icon
-- Inputs:   v_tenant_codes       - array of tenant codes
-- Output:   Update rows in task.tenant_task_category and task_reward; logs progress for Watco
-- Story:    RES-1106 and RES-124
-- Author:   Kawalpreet Kaur
-- Date:     08-12-2025
--===============================================================================
DO $$
DECLARE
    -- 🔹 ARRAY of tenant codes instead of a single tenant code
    v_tenant_codes TEXT[] := ARRAY['<WATCO_TENANT_CODE>', '<WATCO_TENANT_CODE>', '<WATCO_TENANT_CODE>'];

    -- 🔹 JSON array input
   v_data JSONB := '[
        { 
			"taskExternalCode": "Step_it_up_2026",
			"actionCategory": "Steps",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Steps.svg" }
		} ,
		{ 
			"taskExternalCode": "Select_your_PCP_2026",
			"actionCategory": "Steps",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Steps.svg" }
		},
		{ 
			"taskExternalCode": "Get_your_zzz''s_2026",
			"actionCategory": "Sleep",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Sleep.svg" }
		},
		{ 
			"taskExternalCode": "Play_trivia_2026",
			"actionCategory": "Health and Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Trivia.svg" }
		},
        { 
			"taskExternalCode": "Read_the_Open_Enrollment_Guide_2026",
			"actionCategory": "Enrollment",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Enrollment.svg" }
		},
        { 
			"taskExternalCode": "Complete_your_2027_Open_Enrollment_2026",
			"actionCategory": "Enrollment",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Enrollment.svg" }
		},
        { 
			"taskExternalCode": "Attend_an_Open_Enrollment_Session_2026",
			"actionCategory": "Enrollment",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Enrollment.svg" }
		},
        { 
			"taskExternalCode": "View_Your_Benefit_Summary_2026",
			"actionCategory": "Enrollment",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Enrollment.svg" }
		},
        { 
			"taskExternalCode": "Explore_your_Benefits_Resources_2026",
			"actionCategory": "Enrollment",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Enrollment.svg" }
		},
        { 
			"taskExternalCode": "Update_your_Contact_Information_2026",
			"actionCategory": "Enrollment",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Enrollment.svg" }
		},
        { 
			"taskExternalCode": "Download_your_Benefits_Contact_Card_2026",
			"actionCategory": "Enrollment",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Enrollment.svg" }
		},
        { 
			"taskExternalCode":"Discover_the_Watco_Benefit_Debit_Card_2026",
			"actionCategory": "Card",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Card.svg" }
		},
        { 
			"taskExternalCode":"Download_Your_Medical_Digital_ID_Card_2026",
			"actionCategory": "Card",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Card.svg" }
		},
        { 
			"taskExternalCode":"Review_your_401(k)_Savings_2026",
			"actionCategory": "Card",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Card.svg" }
		},
        { 
			"taskExternalCode": "Download_the_First_Stop_Health_App_2026",
			"actionCategory": "Health",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Health.svg" }
		},
        { 
			"taskExternalCode": "Download_the_Watco_Benefitplace_App_2026",
			"actionCategory": "Health",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Health.svg" }
		},
        { 
			"taskExternalCode": "Check_Out_My_Health_Novel_2026",
			"actionCategory": "Health",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Health.svg" }
		},
        { 
			"taskExternalCode": "How_to_Choose_a_401(k)_Beneficiary_2026",
			"actionCategory": "Beneficiary",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Beneficiary.svg" }
		},
        { 
			"taskExternalCode": "Review_your_Life_Insurance_Beneficiary_2026",
			"actionCategory": "Beneficiary",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Beneficiary.svg" }
		},
        { 
			"taskExternalCode": "Review_your_Beneficiary_with_BOK_2026",
			"actionCategory": "Beneficiary",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Beneficiary.svg" }
		},
        { 
			"taskExternalCode": "Confirm_your_Life_Insurance_Beneficiary_2026",
			"actionCategory": "Beneficiary",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Beneficiary.svg" }
		},
        { 
			"taskExternalCode": "Learn_about_PHM_-_Clear_Cancer_2026",
			"actionCategory": "Positivity",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Positivity.svg" }
		},
        { 
			"taskExternalCode": "Wellness_Services_Covered_at_100%_2026",
			"actionCategory": "Positivity",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Positivity.svg" }
		},
        { 
			"taskExternalCode": "Medicare_Support_Program_2026",
			"actionCategory": "Positivity",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Positivity.svg" }
		},
        { 
			"taskExternalCode": "Check_Out_Watco_Team_Member_Discounts_2026",
			"actionCategory": "Positivity",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-Positivity.svg" }
		},
        { 
			"taskExternalCode": "Complete_a_Yearly_Dental_Exam_2026",
			"actionCategory": "Preventive Care",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-PreventiveCare.svg" }
		},
        { 
			"taskExternalCode": "Get_a_Recommended_Preventive_Screening_2026",
			"actionCategory": "Preventive Care",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-PreventiveCare.svg" }
		},
        { 
			"taskExternalCode": "Complete_your_Annual_Wellness_Physical_2026",
			"actionCategory": "Preventive Care",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-PreventiveCare.svg" }
		},
        { 
			"taskExternalCode": "Complete_a_Yearly_Eye_Exam_2026",
			"actionCategory": "Preventive Care",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-PreventiveCare.svg" }
		},
        { 
			"taskExternalCode": "Explore_Lucet_Resources_2026",
			"actionCategory": "Behavioral Health",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-BehavioralHealth.svg" }
		},
        { 
			"taskExternalCode": "Explore_BetterHelp_2026",
			"actionCategory": "Behavioral Health",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-BehavioralHealth.svg" }
		},
        { 
			"taskExternalCode": "Read_the_Monthly_Lucet_EAP_Newsletter_2026",
			"actionCategory": "Behavioral Health",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-BehavioralHealth.svg" }
		},
        { 
			"taskExternalCode": "Up_Skill_Yourself_2026",
			"actionCategory": "Company Culture",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-CompanyCulture.svg" }
		},
        { 
			"taskExternalCode": "Check_Out_the_New_Watco_Dispatch_2026",
			"actionCategory": "Company Culture",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-CompanyCulture.svg" }
		},
        { 
			"taskExternalCode": "Download_BOK_App_2026",
			"actionCategory": "Financial Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-FinancialWellness.svg" }
		},
        { 
			"taskExternalCode": "Learn_how_to_Enroll_&_Access_your_401(k)_2026",
			"actionCategory": "Financial Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-FinancialWellness.svg" }
		},
        { 
			"taskExternalCode": "Explore_financial_wellness_tools_2026",
			"actionCategory": "Financial Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-FinancialWellness.svg" }
		},
        { 
			"taskExternalCode":"Save_for_Retirement_2026",
			"actionCategory": "Financial Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-FinancialWellness.svg" }
		},
        { 
			"taskExternalCode": "Explore_an_HSA_2026",
			"actionCategory": "Financial Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-FinancialWellness.svg" }
		},
        { 
			"taskExternalCode": "Make_a_budget_2026",
			"actionCategory": "Financial Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-FinancialWellness.svg" }
		},
        { 
			"taskExternalCode": "Explore_the_Benefits_of_an_FSA_2026",
			"actionCategory": "Financial Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Watco-FinancialWellness.svg" }
		}
    ]';

    rec JSONB;
    v_task_external_code TEXT;
    v_action_category TEXT;
    v_task_id BIGINT;
    v_task_category_id BIGINT;
    v_updated_count INT;
    v_resource_json JSONB;
    v_existing_id BIGINT;

    v_tenant TEXT;
BEGIN
    -- Loop through each tenant code
    FOREACH v_tenant IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE '🚀 Processing tenant: %', v_tenant;

        -- Loop through each JSON entry
        FOR rec IN SELECT * FROM jsonb_array_elements(v_data)
        LOOP
            v_task_external_code := rec->>'taskExternalCode';
            v_action_category := rec->>'actionCategory';
            v_resource_json := rec->'resourceURL';

            -- 1️⃣ Get task_id
            SELECT tr.task_id
            INTO v_task_id
            FROM task.task_reward tr
            WHERE tr.task_external_code = v_task_external_code
              AND tr.tenant_code = v_tenant
              AND tr.delete_nbr = 0;

            IF v_task_id IS NULL THEN
                RAISE NOTICE '❌ Task not found: task_external_code=% tenant=%', v_task_external_code, v_tenant;
                CONTINUE;
            END IF;

            -- 2️⃣ Get category_id
            SELECT tc.task_category_id
            INTO v_task_category_id
            FROM task.task_category tc
            WHERE tc.task_category_name = v_action_category
              AND tc.delete_nbr = 0;

            IF v_task_category_id IS NULL THEN
                RAISE NOTICE '⚠️ Category not found: % (Task=%)', v_action_category, v_task_external_code;
                CONTINUE;
            END IF;

            -- 3️⃣ Update task table
            UPDATE task.task
            SET task_category_id = v_task_category_id,
                update_ts = NOW(),
                update_user = 'SYSTEM'
            WHERE task_id = v_task_id
              AND delete_nbr = 0;

            GET DIAGNOSTICS v_updated_count = ROW_COUNT;

            IF v_updated_count > 0 THEN
                RAISE NOTICE '✅ Updated task_id=% (task=%) category=% tenant=%',
                    v_task_id, v_task_external_code, v_action_category, v_tenant;
            ELSE
                RAISE NOTICE '⚠️ No update needed task_id=% (task=%)', v_task_id, v_task_external_code;
            END IF;

            -- 4️⃣ Insert/Update tenant_task_category
            SELECT ttc.tenant_task_category_id
            INTO v_existing_id
            FROM task.tenant_task_category ttc
            WHERE ttc.task_category_id = v_task_category_id
              AND ttc.tenant_code = v_tenant
              AND ttc.delete_nbr = 0;

            IF v_existing_id IS NOT NULL THEN
                UPDATE task.tenant_task_category
                SET resource_json = v_resource_json,
                    update_ts = NOW(),
                    update_user = 'SYSTEM'
                WHERE tenant_task_category_id = v_existing_id;

                RAISE NOTICE '🔄 Updated tenant_task_category_id=% category=% tenant=%',
                    v_existing_id, v_action_category, v_tenant;
            ELSE
                INSERT INTO task.tenant_task_category (
                    task_category_id,
                    tenant_code,
                    resource_json,
                    create_ts,
                    create_user,
                    delete_nbr
                )
                VALUES (
                    v_task_category_id,
                    v_tenant,
                    v_resource_json,
                    NOW(),
                    'SYSTEM',
                    0
                );

                RAISE NOTICE '➕ Inserted new tenant_task_category for category=% tenant=%',
                    v_action_category, v_tenant;
            END IF;

        END LOOP;

        RAISE NOTICE '🎯 Completed tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🎉 ALL TENANTS PROCESS COMPLETED!';
END $$;

--===============================================================================
-- Script:   inserting new tenant_task_category action icon
-- Inputs:   v_tenant_codes       - array of tenant codes
-- Output:   Update rows in task.tenant_task_category and task_reward; logs progress for NAVITUS
-- Story:    RES-1106 and RES-124
-- Author:   Kawalpreet Kaur
-- Date:     08-12-2025
--===============================================================================
DO $$
DECLARE
    -- 🔹 ARRAY of tenant codes instead of a single tenant code
    v_tenant_codes TEXT[] := ARRAY['<NAVITUS_TENANT_CODE>', '<NAVITUS_TENANT_CODE>', '<NAVITUS_TENANT_CODE>'];

    -- 🔹 JSON array input
   v_data JSONB := '[
        { 
			"taskExternalCode": "step_it_up_2026",
			"actionCategory": "Steps",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Steps.svg" }
		},{ 
			"taskExternalCode": "get_acti_outd_2026",
			"actionCategory": "Steps",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Steps.svg" }
		} ,
		{ 
			"taskExternalCode": "be_mind_of_what_you_eat_2026",
			"actionCategory": "Healthy eating",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-HealthyEating.svg" }
		} ,
		{ 
			"taskExternalCode":"eat_more_seed_and_nuts_2026",
			"actionCategory": "Healthy eating",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-HealthyEating.svg" }
		} ,
		{ 
			"taskExternalCode": "try_heal_reci_2026",
			"actionCategory": "Healthy eating",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-HealthyEating.svg" }
		},
		{ 
			"taskExternalCode": "get_your_z_s_2026",
			"actionCategory": "Sleep",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Sleep.svg" }
		},
		{ 
			"taskExternalCode": "play_mont_triv_2026",
			"actionCategory": "Trivia",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Trivia.svg" }
		},
		{ 
			"taskExternalCode": "reth_your_drin_2026",
			"actionCategory": "Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Wellness.svg" }
		},
		{ 
			"taskExternalCode": "live_a_life_of_grat_2026",
			"actionCategory": "Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Wellness.svg" }
		},
		{ 
			"taskExternalCode": "boos_your_resi_conn_tech_free_2026",
			"actionCategory": "Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Wellness.svg" }
		},
		{ 
			"taskExternalCode": "medi_to_boos_your_well_2026",
			"actionCategory": "Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Wellness.svg" }
		},
        { 
			"taskExternalCode": "stre_your_body_2026",
			"actionCategory": "Strength",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Strength.svg" }
		},
        { 
			"taskExternalCode": "volu_your_time_2026",
			"actionCategory": "Positivity",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Positivity.svg" }
		},
        { 
			"taskExternalCode": "take_adva_of_cost_disc_2026",
			"actionCategory": "Shopping",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Shopping.svg" }
		},
        { 
			"taskExternalCode": "get_your_flu_vacc_2026",
			"actionCategory": "Vaccine",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Vaccine.svg" }
		},
        { 
			"taskExternalCode": "save_on_pres_with_cost_2026",
			"actionCategory": "Pharmacy",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Pharmacy.svg" }
		},
        { 
			"taskExternalCode": "comp_your_2026_open_enro_2026",
			"actionCategory": "Benefits",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Benefits.svg" }
		},
        { 
			"taskExternalCode": "conn_with_co_work_2026",
			"actionCategory": "Company Culture",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-CompanyCulture.svg" }
		},
        { 
			"taskExternalCode":"up_skil_your_2026",
			"actionCategory": "Company Culture",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-CompanyCulture.svg" }
		},
        { 
			"taskExternalCode":"sche_your_care_conv_2026",
			"actionCategory": "Work",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Work.svg" }
		},
        { 
			"taskExternalCode":"take_the_comp_core_asse_2026",
			"actionCategory": "Work",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-Work.svg" }
		},
        { 
			"taskExternalCode": "comp_a_dent_exam_2026",
			"actionCategory": "Preventive Care",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-PreventiveCare.svg" }
		},
        { 
			"taskExternalCode": "get_a_reco_prev_scre_2026",
			"actionCategory": "Preventive Care",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-PreventiveCare.svg" }
		},
        { 
			"taskExternalCode": "are_you_on_trac_for_reti_2026",
			"actionCategory": "Financial Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-FinancialWellness.svg" }
		},
        { 
			"taskExternalCode":"be_fina_savv_2026",
			"actionCategory": "Financial Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-FinancialWellness.svg" }
		},
        { 
			"taskExternalCode": "meet_with_a_fina_advi_2026",
			"actionCategory": "Financial Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-FinancialWellness.svg" }
		},
        { 
			"taskExternalCode": "savi_for_your_reti_2026",
			"actionCategory": "Financial Wellness",
			"resourceURL" : { "taskIconUrl": "/assets/icons/Navitus-FinancialWellness.svg" }
		}
    ]';

    rec JSONB;
    v_task_external_code TEXT;
    v_action_category TEXT;
    v_task_id BIGINT;
    v_task_category_id BIGINT;
    v_updated_count INT;
    v_resource_json JSONB;
    v_existing_id BIGINT;

    v_tenant TEXT;
BEGIN
    -- Loop through each tenant code
    FOREACH v_tenant IN ARRAY v_tenant_codes
    LOOP
        RAISE NOTICE '🚀 Processing tenant: %', v_tenant;

        -- Loop through each JSON entry
        FOR rec IN SELECT * FROM jsonb_array_elements(v_data)
        LOOP
            v_task_external_code := rec->>'taskExternalCode';
            v_action_category := rec->>'actionCategory';
            v_resource_json := rec->'resourceURL';

            -- 1️⃣ Get task_id
            SELECT tr.task_id
            INTO v_task_id
            FROM task.task_reward tr
            WHERE tr.task_external_code = v_task_external_code
              AND tr.tenant_code = v_tenant
              AND tr.delete_nbr = 0;

            IF v_task_id IS NULL THEN
                RAISE NOTICE '❌ Task not found: task_external_code=% tenant=%', v_task_external_code, v_tenant;
                CONTINUE;
            END IF;

            -- 2️⃣ Get category_id
            SELECT tc.task_category_id
            INTO v_task_category_id
            FROM task.task_category tc
            WHERE tc.task_category_name = v_action_category
              AND tc.delete_nbr = 0;

            IF v_task_category_id IS NULL THEN
                RAISE NOTICE '⚠️ Category not found: % (Task=%)', v_action_category, v_task_external_code;
                CONTINUE;
            END IF;

            -- 3️⃣ Update task table
            UPDATE task.task
            SET task_category_id = v_task_category_id,
                update_ts = NOW(),
                update_user = 'SYSTEM'
            WHERE task_id = v_task_id
              AND delete_nbr = 0;

            GET DIAGNOSTICS v_updated_count = ROW_COUNT;

            IF v_updated_count > 0 THEN
                RAISE NOTICE '✅ Updated task_id=% (task=%) category=% tenant=%',
                    v_task_id, v_task_external_code, v_action_category, v_tenant;
            ELSE
                RAISE NOTICE '⚠️ No update needed task_id=% (task=%)', v_task_id, v_task_external_code;
            END IF;

            -- 4️⃣ Insert/Update tenant_task_category
            SELECT ttc.tenant_task_category_id
            INTO v_existing_id
            FROM task.tenant_task_category ttc
            WHERE ttc.task_category_id = v_task_category_id
              AND ttc.tenant_code = v_tenant
              AND ttc.delete_nbr = 0;

            IF v_existing_id IS NOT NULL THEN
                UPDATE task.tenant_task_category
                SET resource_json = v_resource_json,
                    update_ts = NOW(),
                    update_user = 'SYSTEM'
                WHERE tenant_task_category_id = v_existing_id;

                RAISE NOTICE '🔄 Updated tenant_task_category_id=% category=% tenant=%',
                    v_existing_id, v_action_category, v_tenant;
            ELSE
                INSERT INTO task.tenant_task_category (
                    task_category_id,
                    tenant_code,
                    resource_json,
                    create_ts,
                    create_user,
                    delete_nbr
                )
                VALUES (
                    v_task_category_id,
                    v_tenant,
                    v_resource_json,
                    NOW(),
                    'SYSTEM',
                    0
                );

                RAISE NOTICE '➕ Inserted new tenant_task_category for category=% tenant=%',
                    v_action_category, v_tenant;
            END IF;

        END LOOP;

        RAISE NOTICE '🎯 Completed tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🎉 ALL TENANTS PROCESS COMPLETED!';
END $$;


