-- ============================================================================
-- 🚀 Script    : Update Task Valid Start Timestamp for 2026
-- 📌 Purpose   : Update `valid_start_ts` in task.task_reward table for matching 
--                tenant_code, task_external_code, and delete_nbr = 0 records.
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-11-13
-- 🧾 Jira      : SUN-1127
-- ⚠️ Inputs    : 
--                1. KP Tenant Codes array
--                2. JSON array containing task_external_code and valid_start_ts
-- 📤 Output    : Updates valid_start_ts column for matching records.
-- 🔗 Script URL: NA
-- 📝 Notes     : 
--                - Ensure you run this in a transaction.
--                - This script assumes valid tenant codes and task_external_codes exist.
--                - delete_nbr = 0 is used to filter active tasks.
-- ============================================================================

DO $$
DECLARE
    -- Replace with your tenant codes
    tenant_codes TEXT[] := ARRAY['<KP-TENANT-CODE>','<KP-TENANT-CODE>'];

    -- JSON array input (as provided)
    tasks JSONB := '{
        "array_task_external_codes": [
            {"task_external_code": "get_movi_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "medi_to_boos_your_well_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "stre_your_body_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "take_a_brea_from_alco_in_janu_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "take_a_brea_from_alco_in_febr_2026", "valid_start_ts": "2026-02-01 00:00:00"},
            {"task_external_code": "take_a_brea_from_alco_in_marc_2026", "valid_start_ts": "2026-03-01 00:00:00"},
            {"task_external_code": "reth_your_drin_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "step_it_up_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "comp_the_tota_heal_asse_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "star_your_well_coac_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "get_your_z_s_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "live_a_life_of_grat_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "play_heal_triv_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "take_a_stro_afte_a_meal_2026", "valid_start_ts": "2026-04-01 00:00:00"},
            {"task_external_code": "volu_your_time_2026", "valid_start_ts": "2026-04-01 00:00:00"},
            {"task_external_code": "eat_the_rain_2026", "valid_start_ts": "2026-07-01 00:00:00"},
            {"task_external_code": "powe_down_befo_bed_2026", "valid_start_ts": "2026-07-01 00:00:00"},
            {"task_external_code": "get_your_flu_vacc_2026", "valid_start_ts": "2026-09-01 00:00:00"},
            {"task_external_code": "eat_more_seed_and_nuts_2026", "valid_start_ts": "2026-10-01 00:00:00"},
            {"task_external_code": "conn_with_thos_who_make_you_smil_2026", "valid_start_ts": "2026-10-01 00:00:00"},
            {"task_external_code": "play_dail_heal_triv_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "play_week_heal_triv_2026", "valid_start_ts": "2026-01-01 00:00:00"},
            {"task_external_code": "shar_your_feed_2026", "valid_start_ts": "2025-08-01 00:00:00"}

        ]
    }'::jsonb;

    task_record JSONB;
    tenant TEXT;
    task_code TEXT;
    new_valid_start_ts TIMESTAMP;
BEGIN
    RAISE NOTICE '🔄 Starting updates for tenant codes: %', tenant_codes;

    -- Loop through tenant codes
    FOREACH tenant IN ARRAY tenant_codes
    LOOP
        RAISE NOTICE '➡️ Processing tenant: %', tenant;

        -- Loop through each JSON task object
        FOR task_record IN SELECT * FROM jsonb_array_elements(tasks->'array_task_external_codes')
        LOOP
            task_code := task_record->>'task_external_code';
            new_valid_start_ts := (task_record->>'valid_start_ts')::timestamp;

            -- Update statement
            UPDATE task.task_reward
            SET valid_start_ts = new_valid_start_ts,
                update_ts = NOW()
            WHERE tenant_code = tenant
              AND task_external_code = task_code
              AND delete_nbr = 0;

            IF FOUND THEN
                RAISE NOTICE '✅ Updated task % for tenant % with valid_start_ts = %', task_code, tenant, new_valid_start_ts;
            ELSE
                RAISE NOTICE '⚠️ No record found for tenant % and task_external_code %', tenant, task_code;
            END IF;
        END LOOP;
    END LOOP;

    RAISE NOTICE '🏁 All updates completed.';
END $$;
