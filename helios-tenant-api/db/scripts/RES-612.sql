-- ============================================================================
-- 🚀 Script    : Update supression_condition.consumer in tenant.flow_step
-- 📌 Purpose   : Append or add 'supression_condition.consumer' node under 'step_config'
--                for onboarding_survey flow_step belonging to a given tenant_code and flow_name.
-- 🧑 Author    : Kumar Sirikonda
-- 📅 Date      : 2025-10-13
-- 🧾 Jira      : 
-- ⚠️ Inputs    : KP_tenant_code
--                flow_name
-- 📤 Output    : Updates tenant.flow_step.step_config JSONB field
-- 🔗 Script URL: NA
-- 📝 Notes     : 
--      - This script only updates/creates the 'supression_condition' key.
--      - Existing keys like 'skip_steps' and 'connected_component' remain untouched.
--      - Idempotent: Running multiple times won't duplicate the condition.
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- INPUT: tenant_code
	
    v_flow_name   TEXT := 'onboarding_flow';
	v_component_name TEXT := 'onboarding_survey';

    v_new_condition JSONB := '{
        "attribute_name": "EligibilityMonths",
        "data_type": "int",
        "operator": "<",
        "attribute_value": "6"
    }'::jsonb; -- JSON node to append

    v_flow_pk BIGINT;
    v_rec RECORD;
    v_consumer_array JSONB;
    v_exists BOOLEAN;
	v_component_pk BIGINT;
BEGIN
    -- Step 1: Get the flow_pk for the given tenant and flow_name
    SELECT pk INTO v_flow_pk
    FROM tenant.flow
    WHERE tenant_code = v_tenant_code
      AND flow_name = v_flow_name
      AND delete_nbr = 0;

    -- If flow not found, raise an error
    IF v_flow_pk IS NULL THEN
        RAISE NOTICE '❌ Error: No flow found for tenant_code=% and flow_name=%', v_tenant_code, v_flow_name;
        RETURN;
    END IF;

    RAISE NOTICE '✅ Found flow_pk: %', v_flow_pk;
	
	-- Step 2: Get the component_pk for the given component_name
	SELECT pk INTO v_component_pk
    FROM tenant.component_catalogue
    WHERE component_name = v_component_name
      AND delete_nbr = 0;

    -- If component not found, raise an error
    IF v_component_pk IS NULL THEN
        RAISE NOTICE '❌ Error: No component found for component_name=%', v_component_name;
        RETURN;
    END IF;

    RAISE NOTICE '✅ Found component_pk: %', v_component_pk;
	
	

    -- Step 3: Loop through all flow_steps for the found flow
    FOR v_rec IN 
        SELECT pk, step_config
        FROM tenant.flow_step
        WHERE flow_fk = v_flow_pk
		AND current_component_catalogue_fk = v_component_pk
        AND delete_nbr = 0
    LOOP
        IF v_rec.step_config IS NULL THEN
            -- Case 1: step_config is NULL → Add new JSON with supression_condition only
            UPDATE tenant.flow_step
            SET step_config = jsonb_build_object(
                'supression_condition', jsonb_build_object(
                    'consumer', jsonb_build_array(v_new_condition)
                )
            )
            WHERE pk = v_rec.pk;

            RAISE NOTICE '🆕 Added new step_config with supression_condition for pk=%', v_rec.pk;

        ELSIF NOT (v_rec.step_config ? 'supression_condition') THEN
            -- Case 2: step_config exists but no supression_condition key
            UPDATE tenant.flow_step
            SET step_config = v_rec.step_config || jsonb_build_object(
                'supression_condition', jsonb_build_object(
                    'consumer', jsonb_build_array(v_new_condition)
                )
            )
            WHERE pk = v_rec.pk;

            RAISE NOTICE '➕Added supression_condition to existing step_config for pk=%', v_rec.pk;

        ELSE
            -- Case 3: supression_condition and consumer already exist → append only if not already present
            v_consumer_array := (v_rec.step_config -> 'supression_condition' -> 'consumer')::jsonb;

            IF v_consumer_array IS NULL THEN
                v_consumer_array := '[]'::jsonb;
            END IF;

            -- Check if the condition already exists in the consumer array
            v_exists := EXISTS (
                SELECT 1
                FROM jsonb_array_elements(v_consumer_array) AS elem
                WHERE elem = v_new_condition
            );

            IF NOT v_exists THEN
                UPDATE tenant.flow_step
                SET step_config = jsonb_set(
                    v_rec.step_config,
                    '{supression_condition,consumer}',
                    v_consumer_array || v_new_condition
                )
                WHERE pk = v_rec.pk;

                RAISE NOTICE '✅ Appended new consumer node under supression_condition for pk=%', v_rec.pk;
            ELSE
                RAISE NOTICE '⚠️ Condition already exists for pk=%, skipping update', v_rec.pk;
            END IF;
        END IF;
    END LOOP;

    RAISE NOTICE '🎯 Completed supression_condition update for tenant_code=% and flow_name=%', v_tenant_code, v_flow_name;
END $$;
