-- ============================================================================
-- 📌 Purpose   : 
--   - Populate `step_config` JSONB column for a main component under a specific tenant
--   - Marks skip_steps = true and sets connected_component references
--
-- 🧑 Author    : Saurabh
-- 📅 Date      : 2025-09-25
-- 🧾 Jira      : BEN-8
--
-- ⚠️ Inputs    : 
--   - v_tenant_code          (Tenant code, e.g., '<HAP-TENANT-CODE>')
--   - v_main_component       (Main component, e.g., 'activate_card_model')
--   - v_connected_components (Array of dependent components, e.g.,
--                               ['dob_verification_screen',
--                                'card_last_4_verification_screen',
--                                'card_activate_success_model'])
--
-- 📤 Output    : 
--   - Updates `tenant.flow_step.step_config` for the given tenant + main component
--   - JSON structure applied:
--        {
--          "skip_steps": true,
--          "connected_component": [ <IDs of  steps for connected component ]
--        }
--
-- 🔗 Script URL: https://github.com/SunnyRewards/helios-tenant-api/blob/develop/db/scripts/BEN-8-UpdateStepConfigForHAP.sql
--
-- 📝 Notes     : 
--   - Idempotent: safe to re-run (overwrites existing JSON)
--   - Assumes all component names exist in tenant.component_catalogue
--   - Scoped by tenant_code to avoid cross-tenant updates
--   - Uses SYSTEM user + current timestamp for audit consistency
-- ============================================================================


DO $$
DECLARE
    v_tenant_code  TEXT := '<HAP-TENANT-CODE>'; -- <<<<<< CHANGE TENANT CODE
    v_main_component TEXT := 'activate_card_model'; 
    v_connected_components TEXT[] := ARRAY[
        'dob_verification_screen',
        'card_last_4_verification_screen',
        'card_activate_success_model'
    ];

    v_cohort_names TEXT[] := ARRAY[
        'hap_flex_rewards',
        'henry_ford_health_flex_rewards'
    ];

    v_cohort_name  TEXT;
    v_cohort_code  TEXT;
    v_connected_ids BIGINT[];
BEGIN
    FOREACH v_cohort_name IN ARRAY v_cohort_names LOOP
        ----------------------------------------------------------------------
        -- Look up cohort_code from cohort.cohort by cohort_name
        ----------------------------------------------------------------------
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
        -- Lookup PKs of flow_steps corresponding to connected components
        ----------------------------------------------------------------------
        SELECT array_agg(fs.pk)
        INTO v_connected_ids
        FROM tenant.flow_step fs
        JOIN tenant.flow f ON f.pk = fs.flow_fk
        JOIN tenant.component_catalogue cc ON cc.pk = fs.current_component_catalogue_fk
        WHERE cc.component_name = ANY(v_connected_components)
          AND cc.delete_nbr = 0
          AND f.tenant_code = v_tenant_code
          AND f.cohort_code = v_cohort_code
          AND f.delete_nbr = 0
          AND fs.delete_nbr = 0;

        -- Validate that all dependent components exist
        IF v_connected_ids IS NULL OR array_length(v_connected_ids,1) <> array_length(v_connected_components,1) THEN
            RAISE EXCEPTION '❌ One or more connected components not found in tenant.flow_step for tenant %, cohort %',
                v_tenant_code, v_cohort_name;
        END IF;

        ----------------------------------------------------------------------
        -- Update step_config only for the given tenant + cohort + main component
        ----------------------------------------------------------------------
        UPDATE tenant.flow_step fs
        SET step_config = jsonb_build_object(
            'skip_steps', true,
            'connected_component', to_jsonb(v_connected_ids)
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

        RAISE NOTICE '✅ Updated step_config for % in tenant % cohort % with connected flow_step IDs %', 
            v_main_component, v_tenant_code, v_cohort_name, v_connected_ids;
    END LOOP;
END $$;
