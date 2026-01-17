-- ROLLBACK SCRIPT for inserted task.task_detail records
DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- replace with your tenant
   v_mapping JSONB := '[
        {"taskExternalCode": "play_heal_triv_2026"},
        {"taskExternalCode": "play_dail_heal_triv_2026"},
        {"taskExternalCode": "play_week_heal_triv_2026"},
        {"taskExternalCode": "volu_your_time_2026"},
        {"taskExternalCode": "powe_down_befo_bed_2026"},
        {"taskExternalCode": "live_a_life_of_grat_2026"},
        {"taskExternalCode": "get_your_z_s_2026"},
        {"taskExternalCode": "star_your_well_coac_2026"},
        {"taskExternalCode": "conn_with_thos_who_make_you_smil_2026"},
        {"taskExternalCode": "shar_your_feed_2026"},
        {"taskExternalCode": "get_your_flu_vacc_2026"},
        {"taskExternalCode": "comp_the_tota_heal_asse_2026"},
        {"taskExternalCode": "step_it_up_2026"},
        {"taskExternalCode": "reth_your_drin_2026"},
        {"taskExternalCode": "take_a_stro_afte_a_meal_2026"},
        {"taskExternalCode": "take_a_brea_from_alco_in_marc_2026"},
        {"taskExternalCode": "take_a_brea_from_alco_in_febr_2026"},
        {"taskExternalCode": "take_a_brea_from_alco_in_janu_2026"},
        {"taskExternalCode": "eat_the_rain_2026"},
        {"taskExternalCode": "eat_more_seed_and_nuts_2026"},
        {"taskExternalCode": "stre_your_body_2026"},
        {"taskExternalCode": "medi_to_boos_your_well_2026"},
        {"taskExternalCode": "get_movi_2026"}
    ]';

    item JSONB;
    v_task_code TEXT;
    v_task_id BIGINT;
BEGIN
    FOR item IN SELECT * FROM jsonb_array_elements(v_mapping)
    LOOP
        v_task_code := item ->> 'taskExternalCode';

        -- get task_id from task_reward
        SELECT tr.task_id
        INTO v_task_id
        FROM task.task_reward tr
        WHERE tr.task_external_code = v_task_code
          AND tr.tenant_code = v_tenant_code and tr.delete_nbr=0
        LIMIT 1;

        IF v_task_id IS NOT NULL THEN
            -- rollback from task_detail
            DELETE FROM task.task_detail
            WHERE task_id = v_task_id
              AND tenant_code = v_tenant_code and delete_nbr=0;

            RAISE NOTICE 'Rolled back task_id % (external_code %)', v_task_id, v_task_code;
        ELSE
            RAISE NOTICE 'No task found for external_code % in tenant %', v_task_code, v_tenant_code;
        END IF;
    END LOOP;
END $$;
