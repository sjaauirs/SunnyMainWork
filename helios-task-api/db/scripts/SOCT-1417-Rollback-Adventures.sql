DO $$
DECLARE
    in_tenant_code   varchar(50) := '<KP-Tenant-Code>';  -- 👈 input
    in_language_code varchar(5)  := 'es';                 -- 👈 input

    v_component_codes text[];
    v_adventure_ids   bigint[];
BEGIN
    RAISE NOTICE '🚀 Starting rollback for tenant_code=% and language_code=%', in_tenant_code, in_language_code;

    -- 🔎 Fetch all component_codes for this tenant & language
    SELECT array_agg(component_code)
      INTO v_component_codes
      FROM cms.component
     WHERE tenant_code = in_tenant_code
       AND language_code = in_language_code
       AND delete_nbr = 0;

    IF v_component_codes IS NULL OR array_length(v_component_codes,1) = 0 THEN
        RAISE NOTICE 'ℹ️ No components found for tenant_code=% and language_code=%', in_tenant_code, in_language_code;
        RETURN;
    END IF;

    -- 🔎 Fetch all adventure_ids linked to those component_codes
    SELECT array_agg(adventure_id)
      INTO v_adventure_ids
      FROM task.adventure
     WHERE cms_component_code = ANY (v_component_codes)
       AND delete_nbr = 0;

    IF v_adventure_ids IS NULL OR array_length(v_adventure_ids,1) = 0 THEN
        RAISE NOTICE 'ℹ️ No adventures found linked to these components';
    ELSE
        RAISE NOTICE '✅ Found % adventures to rollback', array_length(v_adventure_ids,1);
    END IF;

    -- ✅ Delete from task.tenant_adventure first
    IF v_adventure_ids IS NOT NULL AND array_length(v_adventure_ids,1) > 0 THEN
        RAISE NOTICE '🗑 Deleting from task.tenant_adventure...';
        DELETE FROM task.tenant_adventure
         WHERE tenant_code = in_tenant_code
           AND adventure_id = ANY (v_adventure_ids);
        RAISE NOTICE '✅ Deleted matching rows from task.tenant_adventure';
    END IF;

    -- ✅ Delete from task.adventure
    IF v_adventure_ids IS NOT NULL AND array_length(v_adventure_ids,1) > 0 THEN
        RAISE NOTICE '🗑 Deleting from task.adventure...';
        DELETE FROM task.adventure
         WHERE adventure_id = ANY (v_adventure_ids);
        RAISE NOTICE '✅ Deleted matching rows from task.adventure';
    END IF;

    -- ✅ Delete from cms.component
    RAISE NOTICE '🗑 Deleting from cms.component...';
    DELETE FROM cms.component
     WHERE component_code = ANY (v_component_codes);
    RAISE NOTICE '✅ Deleted matching rows from cms.component';

    RAISE NOTICE '🎯 Rollback completed successfully for tenant_code=% and language_code=%', in_tenant_code, in_language_code;
END;
$$ LANGUAGE plpgsql;
