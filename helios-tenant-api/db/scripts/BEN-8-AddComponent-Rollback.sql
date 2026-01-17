-- ============================================================================
-- 🚀 Script    : Rollback UI Component Types and Component Catalogue Entries
-- 📌 Purpose   : 
--   - Removes specified component_names from tenant.component_catalogue
--   - Removes the component_type (e.g., 'ui_component') if no other catalogue entries reference it
-- 🧑 Author    : Rakesh Pernati
-- 📅 Date      : 2025-09-24
-- 🧾 Jira      : BEN-573
-- ⚠️ Inputs    : 
--       v_component_type   (Component type, e.g., 'ui_component')
--       v_component_names  (Array of component names to remove from tenant.component_catalogue)
-- 📤 Output    : 
--       Deletes component_catalogue rows and, if safe, deletes the component_type
-- 🔗 Script URL: https://github.com/SunnyRewards/helios-tenant-api/blob/develop/db/scripts/BEN-573_remove_ui_component_catalogue.sql
-- 📝 Notes     : 
--   - Idempotent: safely skips non-existent entries
--   - Removes component_type only if no component_catalogue rows reference it
-- ============================================================================

DO $$
DECLARE
    v_component_type   TEXT := 'ui_component'; -- input: component type
    v_component_names  TEXT[] := ARRAY[
        'activate_card_model',
        'card_activate_success_model',
        'rewards_splash_screen'
    ];

    v_ui_component_type BIGINT;
    v_component_id      BIGINT;
    v_name              TEXT;
    v_catalogue_count   INT;
BEGIN
    -- Ensure component_type exists
    SELECT pk
    INTO v_ui_component_type
    FROM tenant.component_type
    WHERE component_type = v_component_type
      AND delete_nbr = 0;

    IF v_ui_component_type IS NULL THEN
        RAISE NOTICE '⚠️ component_type "%" does not exist, nothing to rollback', v_component_type;
        RETURN;
    END IF;

    -- Loop through component_names to delete
    FOREACH v_name IN ARRAY v_component_names LOOP
        SELECT pk
        INTO v_component_id
        FROM tenant.component_catalogue
        WHERE component_name = v_name
          AND component_type_fk = v_ui_component_type
          AND delete_nbr = 0;

        IF v_component_id IS NULL THEN
            RAISE NOTICE '⚠️ component_name "%" not found under component_type "%"', v_name, v_component_type;
        ELSE
            DELETE FROM tenant.component_catalogue
            WHERE pk = v_component_id;

            RAISE NOTICE '🗑️ Deleted component_name "%", pk = %, under component_type_fk = %',
                v_name, v_component_id, v_ui_component_type;
        END IF;
    END LOOP;

    -- Check if any catalogue entries remain for this component_type
    SELECT COUNT(*) INTO v_catalogue_count
    FROM tenant.component_catalogue
    WHERE component_type_fk = v_ui_component_type
      AND delete_nbr = 0;

    IF v_catalogue_count = 0 THEN
        DELETE FROM tenant.component_type
        WHERE pk = v_ui_component_type
          AND delete_nbr = 0;

        RAISE NOTICE '🗑️ Deleted component_type "%", pk = % (no remaining catalogue entries)', 
            v_component_type, v_ui_component_type;
    ELSE
        RAISE NOTICE '🔗 component_type "%" retained because % catalogue entries still reference it', 
            v_component_type, v_catalogue_count;
    END IF;
END;
$$;
