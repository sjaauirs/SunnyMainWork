-- ============================================================================
-- Script Name : Update CTA Button Text to Spanish
-- Author      : Vinod Kumar Ullaganti
-- Created On  : 2025-07-18
-- JIRA ID     : SOCT-1372
-- Description : 
--   - Updates 'Schedule Now' to 'Programar ahora' in the `task.task_detail` table
--     for a given tenant and Spanish language records.
--   - Ensures tenant exists before performing the update.
--   - Note: 'Schedule Now' does not have an approved translation; using 
--     ChatGPT-suggested Spanish translation: 'Programar ahora'.
-- ============================================================================
DO
$$
DECLARE
    v_tenant_code TEXT := '<TENANT_CODE>'; -- Target tenant (replace with actual code)
    v_old_text    TEXT := 'Schedule Now';
    v_new_text    TEXT := 'Programar ahora'; -- Spanish translation (ChatGPT suggestion)
    v_exists      BOOLEAN;
    v_updated     INTEGER;
BEGIN
    -- Step 1: Validate tenant code
    SELECT EXISTS (
        SELECT 1 
        FROM tenant.tenant 
        WHERE tenant_code = v_tenant_code
    ) INTO v_exists;

    IF v_exists THEN
        RAISE NOTICE 'Tenant % exists. Proceeding with update...', v_tenant_code;

        -- Step 2: Perform the update
        UPDATE task.task_detail
        SET task_cta_button_text = v_new_text
        WHERE task_cta_button_text = v_old_text
          AND tenant_code = v_tenant_code
          AND language_code = 'es'
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_updated = ROW_COUNT;
        RAISE NOTICE 'Total records updated: %', v_updated;

    ELSE
        RAISE WARNING 'Tenant % not found. Skipping update.', v_tenant_code;
    END IF;
END
$$;
