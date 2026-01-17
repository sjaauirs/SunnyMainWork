-- ============================================================================
-- 🚀 Script       : Update  description for en-US Components (adventure-driven)
-- 📌 Purpose      : Reads adventures for each tenant, extracts the adventure key
--                   (e.g. "adventure:healthy-eating"), maps it to an image file,
--                   and updates `data.details.graphics.URL` inside cms.component.data_json
--                   for the matching component (by component_code + tenant + language).
-- 👨‍💻 Author       : Kumar Sirikonda
-- 📅 Date         : 2025-11-20
-- 🧾 Jira         : SUN-686
--
-- ⚠️ Inputs       :
--      • v_tenant_codes → Array of tenant identifiers
--
-- 📤 Output       :
--      • Updates only `data_json -> data -> details -> description`
--      • Preserves the rest of the JSON structure
--      • Logs Updated / NotFound / Skipped / Errors
--
-- 📝 Notes        :
--      • Safe and idempotent — repeated runs are safe
--      • Uses jsonb_set() + COALESCE for safe JSON updates
--      • Extend image mapping in the CASE block as needed
-- ============================================================================

DO
$$
DECLARE
    ---------------------------------------------------------------------------
    -- INPUTS: tenants, environment, languages
    ---------------------------------------------------------------------------
    v_tenant_codes TEXT[] := ARRAY[
        '<KP-TENANT-CODE>',
        '<KP-TENANT-CODE>'
    ];

    v_language TEXT;
   
    v_component_type_code TEXT := 'cty-9a7b2c1d4e8f4735a6c5b9d12f3e6a89';
    v_component_type_id BIGINT;
    
    v_adv_rec RECORD;         -- will hold adventure_config_json, cms_component_code, adventure_id

    v_component_id BIGINT;
    v_existing_json JSONB;
    v_cohort TEXT;
    v_adventure_name TEXT;
	v_adventure_description TEXT;

    v_user_id TEXT := 'SYSTEM';
    v_now TIMESTAMP := NOW();

    v_total_updated   INT := 0;
    v_total_notfound  INT := 0;
    v_total_skipped   INT := 0;
    v_total_errors    INT := 0;

    v_tenant TEXT;

BEGIN

    -- =======================================================================
    -- Resolve component type id (optional, used for additional safety)
    -- =======================================================================
    SELECT component_type_id
    INTO v_component_type_id
    FROM cms.component_type
    WHERE component_type_code = v_component_type_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_component_type_id IS NULL THEN
        RAISE EXCEPTION 'Component type not found for code: %', v_component_type_code;
    END IF;


    -- =======================================================================
    -- MAIN LOOPS: language -> tenant -> adventures
    -- =======================================================================
	FOREACH v_tenant IN ARRAY v_tenant_codes
        LOOP
            RAISE NOTICE '🏢 Processing tenant: %', v_tenant;

            -- Iterate adventures linked to this tenant (via tenant_adventure)
            FOR v_adv_rec IN
                SELECT a.adventure_id, a.adventure_config_json, a.cms_component_code
                FROM task.adventure a
                INNER JOIN task.tenant_adventure ta ON ta.adventure_id = a.adventure_id
                WHERE ta.tenant_code = v_tenant
                  AND a.delete_nbr = 0
                  AND ta.delete_nbr = 0
            LOOP
                BEGIN
                    -- Extract first adventure cohort that starts with 'adventure:'
                    v_cohort := (
                        SELECT elem
                        FROM jsonb_array_elements_text(COALESCE(v_adv_rec.adventure_config_json->'cohorts','[]'::jsonb)) AS elem
                        WHERE elem LIKE 'adventure:%'
                        LIMIT 1
                    );

                    IF v_cohort IS NULL THEN
                        v_total_skipped := v_total_skipped + 1;
                        RAISE NOTICE '⚠️ Skipping adventure_id=% (no "adventure:*" cohort) component_code=%',
                            v_adv_rec.adventure_id, v_adv_rec.cms_component_code;
                        CONTINUE;
                    END IF;

                    -- get the part after the colon: e.g. 'adventure:healthy-eating' -> 'healthy-eating'
                    v_adventure_name := lower(split_part(v_cohort, ':', 2));

                    -- Map adventure name to image file (extend this CASE as needed)
                    IF v_adventure_name = 'healthy-eating' THEN
						v_adventure_description:= 'This adventure focuses on how to eat healthier, discover nutritious foods, and build lasting habits.';

					ELSIF v_adventure_name = 'self-care' THEN
						v_adventure_description:= 'The Self-care and wellness adventure is here to help you make your mental health just as strong as your physical health.';

					ELSIF v_adventure_name = 'exercise' THEN
						v_adventure_description:= 'The Fitness and exercise adventure is here to keep you feeling strong, energized, and ready to take on anything.';

					ELSE
						RAISE NOTICE '⚠️ Skipping adventure_id=% (unmapped adventure: %)  component_code=%',
							v_adv_rec.adventure_id, v_adventure_name, v_adv_rec.cms_component_code;
						v_total_skipped := v_total_skipped + 1;
						CONTINUE;  -- valid here
					END IF;

                    -- find matching cms.component by component_code + tenant + language
                    SELECT component_id, data_json, language_code
                    INTO v_component_id, v_existing_json, v_language
                    FROM cms.component
                    WHERE component_code = v_adv_rec.cms_component_code
                      AND tenant_code = v_tenant
                      AND delete_nbr = 0
					  AND language_code = 'en-US'
                    LIMIT 1;

                    IF v_component_id IS NULL THEN
                        v_total_notfound := v_total_notfound + 1;
                        RAISE NOTICE '❌ No CMS component found for component_code=% tenant=% language=% (adventure=%)',
                            v_adv_rec.cms_component_code, v_tenant, v_language, v_adventure_name;
                        CONTINUE;
                    END IF;

                    -- update graphics.URL safely (preserve other JSON)
                   UPDATE cms.component
						SET data_json = jsonb_set(
								COALESCE(v_existing_json, '{}'::jsonb),
								'{data,details,description}',
								to_jsonb(v_adventure_description::text),
								true
							),
							update_ts = v_now,
							update_user = v_user_id
						WHERE component_id = v_component_id;

                    v_total_updated := v_total_updated + 1;
                    RAISE NOTICE '✅ Updated component_id=% component_code=% tenant=% adventure=%',
                        v_component_id, v_adv_rec.cms_component_code, v_tenant, v_adventure_name;

                EXCEPTION WHEN OTHERS THEN
                    v_total_errors := v_total_errors + 1;
                    RAISE WARNING '⚠️ Error processing adventure_id=% component_code=% tenant=% lang=% → %',
                        v_adv_rec.adventure_id, v_adv_rec.cms_component_code, v_tenant, v_language, SQLERRM;
                    -- continue processing other adventures
                END;
            END LOOP; -- adventures
        END LOOP; -- tenants

    -- =======================================================================
    -- SUMMARY
    -- =======================================================================
    RAISE NOTICE '--- Summary ---';
    RAISE NOTICE 'Updated : %', v_total_updated;
    RAISE NOTICE 'NotFound: %', v_total_notfound;
    RAISE NOTICE 'Skipped : % (no adventure cohort)', v_total_skipped;
    RAISE NOTICE 'Errors  : %', v_total_errors;
    RAISE NOTICE '✅ Completed adventure -> graphics URL update.';

END;
$$;