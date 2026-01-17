-- ============================================================================
-- 🚀 Script    : Ensure UI Component Types and Component Catalogue Entries
-- 📌 Purpose   : 
--   - Ensures that a given component_type (`ui_component`) exists in tenant.component_type
--   - Ensures that a set of component_names exist in tenant.component_catalogue
--   - Inserts missing entries and logs whether records were created or already existed
-- 🧑 Author    : Saurabh
-- 📅 Date      : 2025-09-24
-- 🧾 Jira      : BEN-8
-- ⚠️ Inputs    : 
--       v_component_type   (Component type, e.g., 'ui_component')
--       v_component_names  (Array of component names to insert into tenant.component_catalogue) 
--		1. activate_card_model
--		2. card_activate_success_model
-- 📤 Output    : 
--       Inserts missing component_type and component_catalogue rows, with notices for each action
-- 🔗 Script URL: https://github.com/SunnyRewards/helios-tenant-api/blob/develop/db/scripts/BEN-8-AddComponent.sql
-- 📝 Notes     : 
--   - Idempotent: does not insert duplicates if entries already exist
--   - Sets is_active = TRUE, create_user = 'SYSTEM', delete_nbr = 0
-- ============================================================================

DO
$$
DECLARE
    v_component_type   TEXT := 'ui_component'; -- input: component type
	-- Input array of component_names in tenant.component_catalogue
    v_component_names  TEXT[] := ARRAY[
        'activate_card_model',
		'card_activate_success_model'
    ];

    v_ui_component_type BIGINT;
    v_component_id      BIGINT;
    v_name              TEXT;
BEGIN
    -- Ensure component_type exists
    SELECT pk
    INTO v_ui_component_type
    FROM tenant.component_type
    WHERE component_type = v_component_type
      AND delete_nbr = 0;

    IF v_ui_component_type IS NULL THEN
        INSERT INTO tenant.component_type (
            component_type, is_active, create_ts, update_ts, create_user, update_user, delete_nbr
        )
        VALUES (
            v_component_type, TRUE, NOW(), NULL, 'SYSTEM', NULL, 0
        )
        RETURNING pk INTO v_ui_component_type;

        RAISE NOTICE '🎉 Inserted component_type "%", pk = %', v_component_type, v_ui_component_type;
    ELSE
        RAISE NOTICE '🔍 component_type "%" already exists with pk = %', v_component_type, v_ui_component_type;
    END IF;

    -- Loop through component_names
    FOREACH v_name IN ARRAY v_component_names LOOP
        SELECT pk
        INTO v_component_id
        FROM tenant.component_catalogue
        WHERE component_name = v_name
          AND delete_nbr = 0;

        IF v_component_id IS NULL THEN
            INSERT INTO tenant.component_catalogue (
                component_type_fk, component_name, is_active, create_ts, update_ts, create_user, update_user, delete_nbr
            )
            VALUES (
                v_ui_component_type, v_name, TRUE, NOW(), NULL, 'SYSTEM', NULL, 0
            )
            RETURNING pk INTO v_component_id;

            RAISE NOTICE '✅ Inserted component_name "%", pk = %, under component_type_fk = %',
                v_name, v_component_id, v_ui_component_type;
        ELSE
            RAISE NOTICE '🔍 component_name "%" already exists with pk = %', v_name, v_component_id;
        END IF;
    END LOOP;

END;
$$