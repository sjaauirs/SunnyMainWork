
-- ============================================================================
-- 📌 Purpose   : Updates step_config for onboarding flow steps (multi-tenant)
-- 🧑 Author    : Saurabh Jaiswal
-- 📅 Date      : 2025-11-13
-- 	  jIra		 : BEN-1807
-- ⚙️ Notes     :
--                 • Must run AFTER onboarding flow creation.
--                 • Uses COALESCE to handle NULL step_config.
--						 SAfe to rerun
-- ============================================================================

DO
$$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        '<NAV-TENANT-CODE>'
        -- Add more tenants if needed
    ];

    v_tenant_code TEXT;
    v_main_component TEXT;
    v_skip_components TEXT[];
    v_connected_ids BIGINT[];
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        RAISE NOTICE '🚀 Updating step_config for tenant: %', v_tenant_code;

        -- CASE 1: activate_card_model
        v_main_component := 'activate_card_model';
        v_skip_components := ARRAY['dob_verification_screen','card_last_4_verification_screen','card_activate_success_model'];

        SELECT array_agg(fs.pk ORDER BY array_position(v_skip_components, cc.component_name))
        INTO v_connected_ids
        FROM tenant.flow_step fs
        JOIN tenant.flow f ON f.pk = fs.flow_fk
        JOIN tenant.component_catalogue cc ON cc.pk = fs.current_component_catalogue_fk
        WHERE cc.component_name = ANY(v_skip_components)
          AND f.tenant_code = v_tenant_code
          AND f.flow_name = 'onboarding_flow'
          AND f.delete_nbr = 0 AND fs.delete_nbr = 0;

        IF v_connected_ids IS NOT NULL THEN
            UPDATE tenant.flow_step fs
            SET step_config = jsonb_set(
                                jsonb_set(COALESCE(fs.step_config, '{}'::jsonb), '{skip_steps}', 'true'::jsonb, true),
                                '{connected_component}', to_jsonb(v_connected_ids), true
                             )
            FROM tenant.component_catalogue cc, tenant.flow f
            WHERE fs.current_component_catalogue_fk = cc.pk
              AND cc.component_name = v_main_component
              AND f.pk = fs.flow_fk
              AND f.tenant_code = v_tenant_code
              AND f.flow_name = 'onboarding_flow'
              AND f.delete_nbr = 0 AND fs.delete_nbr = 0;
        END IF;

        -- CASE 2: dob_verification_screen
        v_main_component := 'dob_verification_screen';
        v_skip_components := ARRAY['card_last_4_verification_screen','card_activate_success_model'];

        SELECT array_agg(fs.pk ORDER BY array_position(v_skip_components, cc.component_name))
        INTO v_connected_ids
        FROM tenant.flow_step fs
        JOIN tenant.flow f ON f.pk = fs.flow_fk
        JOIN tenant.component_catalogue cc ON cc.pk = fs.current_component_catalogue_fk
        WHERE cc.component_name = ANY(v_skip_components)
          AND f.tenant_code = v_tenant_code
          AND f.flow_name = 'onboarding_flow'
          AND f.delete_nbr = 0 AND fs.delete_nbr = 0;

        IF v_connected_ids IS NOT NULL THEN
            UPDATE tenant.flow_step fs
            SET step_config = jsonb_set(
                                jsonb_set(COALESCE(fs.step_config, '{}'::jsonb), '{skip_steps}', 'true'::jsonb, true),
                                '{connected_component}', to_jsonb(v_connected_ids), true
                             )
            FROM tenant.component_catalogue cc, tenant.flow f
            WHERE fs.current_component_catalogue_fk = cc.pk
              AND cc.component_name = v_main_component
              AND f.pk = fs.flow_fk
              AND f.tenant_code = v_tenant_code
              AND f.flow_name = 'onboarding_flow'
              AND f.delete_nbr = 0 AND fs.delete_nbr = 0;
        END IF;

        -- CASE 3: card_last_4_verification_screen
        v_main_component := 'card_last_4_verification_screen';
        v_skip_components := ARRAY['card_activate_success_model'];

        SELECT array_agg(fs.pk ORDER BY array_position(v_skip_components, cc.component_name))
        INTO v_connected_ids
        FROM tenant.flow_step fs
        JOIN tenant.flow f ON f.pk = fs.flow_fk
        JOIN tenant.component_catalogue cc ON cc.pk = fs.current_component_catalogue_fk
        WHERE cc.component_name = ANY(v_skip_components)
          AND f.tenant_code = v_tenant_code
          AND f.flow_name = 'onboarding_flow'
          AND f.delete_nbr = 0 AND fs.delete_nbr = 0;

        IF v_connected_ids IS NOT NULL THEN
            UPDATE tenant.flow_step fs
            SET step_config = jsonb_set(
                                jsonb_set(COALESCE(fs.step_config, '{}'::jsonb), '{skip_steps}', 'true'::jsonb, true),
                                '{connected_component}', to_jsonb(v_connected_ids), true
                             )
            FROM tenant.component_catalogue cc, tenant.flow f
            WHERE fs.current_component_catalogue_fk = cc.pk
              AND cc.component_name = v_main_component
              AND f.pk = fs.flow_fk
              AND f.tenant_code = v_tenant_code
              AND f.flow_name = 'onboarding_flow'
              AND f.delete_nbr = 0 AND fs.delete_nbr = 0;
        END IF;

        RAISE NOTICE '✅ Completed step_config updates for tenant: %', v_tenant_code;
    END LOOP;

    RAISE NOTICE '🏁 All tenants processed successfully';
END
$$;