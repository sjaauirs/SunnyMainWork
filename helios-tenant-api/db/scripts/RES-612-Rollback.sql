-- ============================================================================ 
-- 🚀 Script    : Rollback supression_condition from tenant.flow_step
-- 📌 Purpose   : Remove the entire 'supression_condition' node from the 'step_config'
--                for onboarding_survey flow_step belonging to a given tenant_code and flow_name.
-- 🧑 Author    : Kumar Sirikonda
-- 📅 Date      : 2025-10-13
-- 🧾 Jira      : 
-- ⚠️ Inputs    : KP_tenant_code
--                flow_name
-- 📤 Output    : Removes supression_condition from tenant.flow_step.step_config JSONB field
-- 🔗 Script URL: NA
-- 📝 Notes     : 
--      - This script removes the entire 'supression_condition' node, including 'consumer'.
--      - Existing keys like 'skip_steps' and 'connected_component' remain untouched.
--      - Idempotent: Running multiple times won't cause any issues if the condition is already removed.
-- ============================================================================

DO $$ 
DECLARE 
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- INPUT: tenant_code
    v_flow_name   TEXT := 'onboarding_flow'; 
    v_component_name TEXT := 'onboarding_survey'; 

    v_flow_pk BIGINT;
    v_rec RECORD;
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

    -- Step 2: Loop through all flow_steps for the found flow
    FOR v_rec IN 
        SELECT pk, step_config
        FROM tenant.flow_step
        WHERE flow_fk = v_flow_pk
        AND current_component_catalogue_fk = (SELECT pk FROM tenant.component_catalogue WHERE component_name = v_component_name AND delete_nbr = 0)
        AND delete_nbr = 0
    LOOP
        -- Case 1: Check if 'supression_condition' exists in step_config
        IF v_rec.step_config ? 'supression_condition' THEN
            -- Case 2: Remove the entire 'supression_condition' node from step_config
            UPDATE tenant.flow_step
            SET step_config = v_rec.step_config - 'supression_condition'
            WHERE pk = v_rec.pk;

            RAISE NOTICE '🗑 Removed supression_condition from step_config for pk=%', v_rec.pk;
        ELSE
            RAISE NOTICE '⚠️ supression_condition not found for pk=%, skipping', v_rec.pk;
        END IF;
    END LOOP;

    RAISE NOTICE '🎯 Completed rollback for supression_condition removal for tenant_code=% and flow_name=%', v_tenant_code, v_flow_name;
END $$;
