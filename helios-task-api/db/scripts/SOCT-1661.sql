-- SOCT-1661
-- Update Learn & Earn to Play healthy trivia
DO
$$
DECLARE
    v_tenant_code TEXT := 'ten-153bd6c47ebe4673a75c71faa22b9eb6'; -- Target tenant (replace with actual code)
    v_old_text    TEXT := 'Learn & Earn';
    v_new_text    TEXT := 'Play healthy trivia'; 
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
        SET task_header = v_new_text, task_description = ''
        WHERE task_header = v_old_text
          AND tenant_code = v_tenant_code
          AND language_code = 'en-US'
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_updated = ROW_COUNT;
        RAISE NOTICE 'Total records updated: %', v_updated;

    ELSE
        RAISE WARNING 'Tenant % not found. Skipping update.', v_tenant_code;
    END IF;
END
$$;


DO
$$
DECLARE
    v_tenant_code TEXT := 'ten-153bd6c47ebe4673a75c71faa22b9eb6'; -- Target tenant (replace with actual code)
    v_old_text    TEXT := 'Aprenda y gane';
    v_new_text    TEXT := 'Participe en la trivia de salud'; 
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
        SET task_header = v_new_text, task_description = ''
        WHERE task_header = v_old_text
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
