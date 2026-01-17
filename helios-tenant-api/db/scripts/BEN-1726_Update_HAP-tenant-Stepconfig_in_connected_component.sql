-- ============================================================================
-- 📌 Purpose   :
--   - Update existing `step_config` JSONB for specific components (activate_card_model,
--     dob_verification_screen, card_last_4_verification_screen) under each tenant
--     and each cohort (processed individually).
--   - Ensures that only `skip_steps` and `connected_component` keys are updated
--     while preserving all other JSON properties.
-- 🧑 Author    : Srikanth Kodam
-- 📅 Date      : 2025-11-11
-- 🧾 Jira      : BEN-1726
-- ⚠️ Inputs    :
--   - v_tenant_codes   : Array of tenant codes (e.g., ['<HAP-TENANT-CODE>'])
--   - v_cohort_names   : Array of cohort names to process
-- 📤 Output    :
--   - Updates `tenant.flow_step.step_config` JSONB fields per cohort.
--   - Safely re-runnable, non-destructive updates.
-- 📝 Notes     :
--   - Preserves all existing JSON keys.
--   - Ensures per-cohort and per-tenant isolation.
--   - Logs progress for each updated component.
-- ============================================================================

DO $$
DECLARE
    -- 🔹 Tenant list
    v_tenant_codes TEXT[] := ARRAY[
        '<HAP-TENANT-CODE>'  
    ];
    v_tenant_code TEXT;

    -- 🔹 Cohorts to process
    v_cohort_names TEXT[] := ARRAY[
        'hap_flex_rewards',
        'henry_ford_health_flex_rewards'
    ];
    v_cohort_name  TEXT;
    v_cohort_code  TEXT;

    -- 🔹 Flow component logic variables
    v_main_component  TEXT;
    v_skip_components TEXT[];
    v_connected_ids   BIGINT[];
BEGIN
    -- 🔁 Iterate each tenant
    FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        RAISE NOTICE '🚀 Processing tenant: %', v_tenant_code;

        -- 🔁 Iterate each cohort under this tenant
        FOREACH v_cohort_name IN ARRAY v_cohort_names LOOP
            RAISE NOTICE '➡️ Processing cohort: %', v_cohort_name;

            -- Step 1️: Get cohort_code
            SELECT cohort_code
            INTO v_cohort_code
            FROM cohort.cohort
            WHERE cohort_name = v_cohort_name
              AND delete_nbr = 0
            LIMIT 1;

            IF v_cohort_code IS NULL THEN
                RAISE EXCEPTION '❌ Cohort "%" not found in cohort.cohort', v_cohort_name;
            END IF;

            ----------------------------------------------------------------------
            -- CASE 1️: activate_card_model
            -- skip_steps = true
            -- connected_component = ['dob_verification_screen','card_last_4_verification_screen','card_activate_success_model']
            ----------------------------------------------------------------------
            v_main_component := 'activate_card_model';
            v_skip_components := ARRAY['dob_verification_screen','card_last_4_verification_screen','card_activate_success_model'];

            SELECT array_agg(fs.pk ORDER BY array_position(v_skip_components, cc.component_name))
            INTO v_connected_ids
            FROM tenant.flow_step fs
            JOIN tenant.flow f ON f.pk = fs.flow_fk
            JOIN tenant.component_catalogue cc ON cc.pk = fs.current_component_catalogue_fk
            WHERE cc.component_name = ANY(v_skip_components)
              AND cc.delete_nbr = 0
              AND f.tenant_code = v_tenant_code
              AND f.cohort_code = v_cohort_code
              AND f.delete_nbr = 0
              AND fs.delete_nbr = 0;

            IF v_connected_ids IS NOT NULL THEN
                UPDATE tenant.flow_step fs
                SET step_config = jsonb_set(
                                    jsonb_set(fs.step_config, '{skip_steps}', 'true'::jsonb, true),
                                    '{connected_component}', to_jsonb(v_connected_ids), true
                                 )
                FROM tenant.component_catalogue cc, tenant.flow f
                WHERE fs.current_component_catalogue_fk = cc.pk
                  AND cc.component_name = v_main_component
                  AND cc.delete_nbr = 0
                  AND f.pk = fs.flow_fk
                  AND f.tenant_code = v_tenant_code
                  AND f.cohort_code = v_cohort_code
                  AND f.delete_nbr = 0
                  AND fs.delete_nbr = 0;

                RAISE NOTICE '✅ Updated step_config for % | Tenant: % | Cohort: % | Connected IDs: %',
                    v_main_component, v_tenant_code, v_cohort_name, v_connected_ids;
            END IF;

            ----------------------------------------------------------------------
            -- CASE 2️: dob_verification_screen
            -- skip_steps = true
            -- connected_component = ['card_last_4_verification_screen','card_activate_success_model']
            ----------------------------------------------------------------------
            v_main_component := 'dob_verification_screen';
            v_skip_components := ARRAY['card_last_4_verification_screen','card_activate_success_model'];

            SELECT array_agg(fs.pk ORDER BY array_position(v_skip_components, cc.component_name))
            INTO v_connected_ids
            FROM tenant.flow_step fs
            JOIN tenant.flow f ON f.pk = fs.flow_fk
            JOIN tenant.component_catalogue cc ON cc.pk = fs.current_component_catalogue_fk
            WHERE cc.component_name = ANY(v_skip_components)
              AND cc.delete_nbr = 0
              AND f.tenant_code = v_tenant_code
              AND f.cohort_code = v_cohort_code
              AND f.delete_nbr = 0
              AND fs.delete_nbr = 0;

            IF v_connected_ids IS NOT NULL THEN
                UPDATE tenant.flow_step fs
                SET step_config = jsonb_set(
                                    jsonb_set(fs.step_config, '{skip_steps}', 'true'::jsonb, true),
                                    '{connected_component}', to_jsonb(v_connected_ids), true
                                 )
                FROM tenant.component_catalogue cc, tenant.flow f
                WHERE fs.current_component_catalogue_fk = cc.pk
                  AND cc.component_name = v_main_component
                  AND cc.delete_nbr = 0
                  AND f.pk = fs.flow_fk
                  AND f.tenant_code = v_tenant_code
                  AND f.cohort_code = v_cohort_code
                  AND f.delete_nbr = 0
                  AND fs.delete_nbr = 0;

                RAISE NOTICE '✅ Updated step_config for % | Tenant: % | Cohort: % | Connected IDs: %',
                    v_main_component, v_tenant_code, v_cohort_name, v_connected_ids;
            END IF;

            ----------------------------------------------------------------------
            -- CASE 3️: card_last_4_verification_screen
            -- skip_steps = true
            -- connected_component = ['card_activate_success_model']
            ----------------------------------------------------------------------
            v_main_component := 'card_last_4_verification_screen';
            v_skip_components := ARRAY['card_activate_success_model'];

            SELECT array_agg(fs.pk ORDER BY array_position(v_skip_components, cc.component_name))
            INTO v_connected_ids
            FROM tenant.flow_step fs
            JOIN tenant.flow f ON f.pk = fs.flow_fk
            JOIN tenant.component_catalogue cc ON cc.pk = fs.current_component_catalogue_fk
            WHERE cc.component_name = ANY(v_skip_components)
              AND cc.delete_nbr = 0
              AND f.tenant_code = v_tenant_code
              AND f.cohort_code = v_cohort_code
              AND f.delete_nbr = 0
              AND fs.delete_nbr = 0;

            IF v_connected_ids IS NOT NULL THEN
                UPDATE tenant.flow_step fs
                SET step_config = jsonb_set(
                                    jsonb_set(fs.step_config, '{skip_steps}', 'true'::jsonb, true),
                                    '{connected_component}', to_jsonb(v_connected_ids), true
                                 )
                FROM tenant.component_catalogue cc, tenant.flow f
                WHERE fs.current_component_catalogue_fk = cc.pk
                  AND cc.component_name = v_main_component
                  AND cc.delete_nbr = 0
                  AND f.pk = fs.flow_fk
                  AND f.tenant_code = v_tenant_code
                  AND f.cohort_code = v_cohort_code
                  AND f.delete_nbr = 0
                  AND fs.delete_nbr = 0;

                RAISE NOTICE '✅ Updated step_config for % | Tenant: % | Cohort: % | Connected IDs: %',
                    v_main_component, v_tenant_code, v_cohort_name, v_connected_ids;
            END IF;

            RAISE NOTICE '🎯 Completed updates for cohort: % in tenant: %', v_cohort_name, v_tenant_code;
            RAISE NOTICE '------------------------------------------------------------';

        END LOOP; -- End cohort loop
        RAISE NOTICE '🏁 Completed all cohorts for tenant: %', v_tenant_code;
        RAISE NOTICE '============================================================';
    END LOOP; -- End tenant loop
END $$;
