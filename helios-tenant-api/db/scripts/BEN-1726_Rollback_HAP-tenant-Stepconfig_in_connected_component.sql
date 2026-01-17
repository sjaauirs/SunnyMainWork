-- ============================================================================
-- 📌 Purpose   :
--   - Rollback script to revert skip_steps and connected_component JSON updates
--     applied by the step_config update script for the following components:
--       1. activate_card_model
--       2. dob_verification_screen
--       3. card_last_4_verification_screen
-- 🧑 Author    : Srikanth Kodam
-- 📅 Date      : 2025-11-11
-- 🧾 Jira      : BEN-1726
-- ⚠️ Inputs    :
--   - v_tenant_codes : Array of tenant codes (e.g., ['<HAP-TENANT-CODE>'])
--   - v_cohort_names : Array of cohort names per tenant
-- 📤 Output    :
--   - Updates `tenant.flow_step.step_config` JSONB for specified components,
--     reverting only the modified fields.
-- 📝 Notes     :
--   - Preserves all other existing JSON properties.
--   - Safe to re-run (idempotent rollback).
--   - Scoped strictly by tenant_code and cohort_code.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes  TEXT[] := ARRAY[
        '<HAP-TENANT-CODE>'
    ];

    v_cohort_names TEXT[] := ARRAY[
        'hap_flex_rewards',
        'henry_ford_health_flex_rewards'
    ];

    v_tenant_code  TEXT;
    v_cohort_name  TEXT;
    v_cohort_code  TEXT;
    v_main_component TEXT;

BEGIN
    -- Loop through each tenant
    FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        RAISE NOTICE '🚀 Processing tenant: %', v_tenant_code;

        -- Loop through each cohort under the current tenant
        FOREACH v_cohort_name IN ARRAY v_cohort_names LOOP
            RAISE NOTICE ' Processing cohort: %', v_cohort_name;

            -- Get cohort_code
            SELECT cohort_code
            INTO v_cohort_code
            FROM cohort.cohort
            WHERE cohort_name = v_cohort_name
              AND delete_nbr = 0
            LIMIT 1;

            IF v_cohort_code IS NULL THEN
                RAISE NOTICE '⚠️ Cohort "%" not found for tenant % — skipping', v_cohort_name, v_tenant_code;
                CONTINUE;
            END IF;

            -- Common rollback logic for each component
            FOR v_main_component IN 
                SELECT unnest(ARRAY[
                    'activate_card_model',
                    'dob_verification_screen',
                    'card_last_4_verification_screen'
                ])
            LOOP
                UPDATE tenant.flow_step fs
                SET step_config = jsonb_set(
                                      jsonb_set(
                                          fs.step_config,
                                          '{skip_steps}', 'false'::jsonb, true
                                      ),
                                      '{connected_component}', '[]'::jsonb, true
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

                RAISE NOTICE '♻️ Rolled back % for tenant % cohort %', v_main_component, v_tenant_code, v_cohort_name;
            END LOOP;

        END LOOP;

    END LOOP;
END $$;
