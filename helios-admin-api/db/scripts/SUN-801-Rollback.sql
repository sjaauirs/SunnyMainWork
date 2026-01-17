-- =================================================================================================================================
-- 🔁 ROLLBACK SCRIPT
-- 📌 Purpose   : Safely revert the CARD_ISSUE_STATUS_UPDATE event handler and script configuration.
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-10-22
-- 🧾 Jira      : SUN-801
-- 📝 Notes     : 
--   - This rollback does not drop data; it sets delete_nbr = 1 for soft delete.
--   - Re-running the main script will automatically restore missing configurations.
--   - Ensure rollback is executed in the same environment as the deployment (QA only).
-- =================================================================================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'HAP-TENANT-CODE'   -- Add other tenant codes here if applicable
    ];
    v_tenant_code TEXT;
    v_event_type TEXT := 'CARD_ISSUE_STATUS_UPDATE';
    v_event_sub_type TEXT := 'AGREEMENTS_VERIFIED';
    v_script_code TEXT := 'src-3434a648b1d748f4ac4bac7d1f9532ff';
    v_script_id INTEGER;
BEGIN
    RAISE NOTICE 'Starting rollback for event handler: % / %', v_event_type, v_event_sub_type;

    -- Identify script ID for reference
    SELECT script_id
    INTO v_script_id
    FROM admin.script
    WHERE script_code = v_script_code
      AND delete_nbr = 0;

    IF v_script_id IS NULL THEN
        RAISE NOTICE 'No active script found with code %, skipping script rollback.', v_script_code;
    ELSE
        UPDATE admin.script
        SET delete_nbr = 1,
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE script_id = v_script_id
          AND delete_nbr = 0;

        RAISE NOTICE 'Marked admin.script entry % (ID: %) as deleted.', v_script_code, v_script_id;
    END IF;

    -- Loop through tenants and soft delete event handler records
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE admin.event_handler_script
        SET delete_nbr = 1,
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND event_type = v_event_type
          AND event_sub_type = v_event_sub_type
          AND delete_nbr = 0;

        IF FOUND THEN
            RAISE NOTICE 'Rolled back event_handler_script for tenant %.', v_tenant_code;
        ELSE
            RAISE NOTICE 'No active event_handler_script found for tenant %, skipping.', v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE 'Rollback process completed for all tenants.';
END
$$;
