DO $$
DECLARE
    -- 🔹 Replace with your tenant code
    v_tenant_code TEXT := '<KP-Tenant-Code>';

    -- 🔹 JSON array input
    v_data JSONB := '[
	{
		"taskExternalCode" : "get_movi_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "medi_to_boos_your_well_2026",
		"actionCategory" : "Behavioral Health"
	},
	{
		"taskExternalCode" : "stre_your_body_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "eat_more_seed_and_nuts_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "eat_the_rain_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "take_a_brea_from_alco_in_janu_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "take_a_brea_from_alco_in_febr_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "take_a_brea_from_alco_in_marc_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "take_a_stro_afte_a_meal_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "reth_your_drin_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "step_it_up_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "comp_the_tota_heal_asse_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "get_your_flu_vacc_2026",
		"actionCategory" : "Preventive Care"
	},
	{
		"taskExternalCode" : "shar_your_feed_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "conn_with_thos_who_make_you_smil_2026",
		"actionCategory" : "Behavioral Health"
	},
	{
		"taskExternalCode" : "star_your_well_coac_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "get_your_z_s_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "live_a_life_of_grat_2026",
		"actionCategory" : "Behavioral Health"
	},
	{
		"taskExternalCode" : "powe_down_befo_bed_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "volu_your_time_2026",
		"actionCategory" : "Behavioral Health"
	},
	{
		"taskExternalCode" : "play_week_heal_triv_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "play_dail_heal_triv_2026",
		"actionCategory" : "Health and Wellness"
	},
	{
		"taskExternalCode" : "play_heal_triv_2026",
		"actionCategory" : "Health and Wellness"
	}
	
]';

    -- Variables for each loop
    rec JSONB;
    v_task_external_code TEXT;
    v_action_category TEXT;
    v_task_id BIGINT;
    v_task_category_id BIGINT;
    v_updated_count INT;
BEGIN
    -- Loop through each record in the JSON array
    FOR rec IN SELECT * FROM jsonb_array_elements(v_data)
    LOOP
        v_task_external_code := rec->>'taskExternalCode';
        v_action_category := rec->>'actionCategory';

        -- 1️ Get task_id from task.task_reward
        SELECT tr.task_id
        INTO v_task_id
        FROM task.task_reward tr
        WHERE tr.task_external_code = v_task_external_code
          AND tr.tenant_code = v_tenant_code
          AND tr.delete_nbr = 0;

        IF v_task_id IS NULL THEN
            RAISE NOTICE '❌ Task not found for task_external_code=% and tenant_code=%', v_task_external_code, v_tenant_code;
            CONTINUE;
        END IF;

        -- 2️Get task_category_id from task.task_category
        SELECT tc.task_category_id
        INTO v_task_category_id
        FROM task.task_category tc
        WHERE tc.task_category_name = v_action_category
          AND tc.delete_nbr = 0;

        IF v_task_category_id IS NULL THEN
            RAISE NOTICE '⚠️ Task category not found for category=% (Task=%)', v_action_category, v_task_external_code;
            CONTINUE;
        END IF;

        -- 3️ Update task.task with found category
        UPDATE task.task
        SET task_category_id = v_task_category_id,
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE task_id = v_task_id
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;

        IF v_updated_count > 0 THEN
            RAISE NOTICE '✅ Updated task_id=% (Task=%) with category=%', v_task_id, v_task_external_code, v_action_category;
        ELSE
            RAISE NOTICE '⚠️ No update performed for task_id=% (Task=%)', v_task_id, v_task_external_code;
        END IF;

    END LOOP;

    RAISE NOTICE '🎉 Task category update process completed!';
END $$;
