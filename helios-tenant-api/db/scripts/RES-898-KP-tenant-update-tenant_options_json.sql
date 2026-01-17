-- ============================================================================
-- 🚀 Script    : Update tenant_option_json → benefitsOptions Menus
-- 📌 Purpose   : Add or update `primaryMenu`, `secondaryMenu`, and `showSelectedPrimaryMenuItem`
--                inside tenant_option_json.benefitsOptions
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-10-17
-- 🧾 Jira      : RES-898
-- ⚠️ Inputs    : KP-TENANT-CODE
-- 📤 Output    : Updates JSON structure; logs status messages via RAISE NOTICE
-- 🔗 Script URL: NA
-- 📝 Notes     : 
--   - Preserves existing keys under benefitsOptions.
--   - Adds menus and flag if not present; updates only specific keys if they exist.
-- ============================================================================

DO
$$
DECLARE
    v_tenant_code   TEXT := '<KP-TENANT-CODE>';  -- Replace with target tenant code
    v_json_input    JSONB := '{
        "primaryMenu": ["myCard","shop","myRewards","healthAdventures"],
        "secondaryMenu": ["agreements","privacyPolicy","signOut"],
        "showSelectedPrimaryMenuItem": true
    }'::jsonb;

    v_option_json   JSONB;
    v_benefits_opts JSONB;
    v_primary_exist BOOLEAN := FALSE;
    v_secondary_exist BOOLEAN := FALSE;
    v_flag_exist    BOOLEAN := FALSE;
BEGIN
    RAISE NOTICE '🔍 Starting script execution for tenant_code: %', v_tenant_code;

    -- ✅ Step 1: Verify tenant existence
    SELECT tenant_option_json INTO v_option_json
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0
    LIMIT 1;

    IF NOT FOUND THEN
        RAISE NOTICE '⚠️ No active tenant found for tenant_code: %', v_tenant_code;
        RETURN;
    END IF;

    RAISE NOTICE '✅ Tenant found. Proceeding with menu updates.';

    -- ✅ Step 2: Extract existing benefitsOptions if available
    v_benefits_opts := COALESCE(v_option_json->'benefitsOptions', '{}'::jsonb);

    -- ✅ Step 3: Check if keys already exist
    v_primary_exist := v_benefits_opts ? 'primaryMenu';
    v_secondary_exist := v_benefits_opts ? 'secondaryMenu';
    v_flag_exist := v_benefits_opts ? 'showSelectedPrimaryMenuItem';

    IF v_primary_exist THEN
        RAISE NOTICE '🔄 primaryMenu exists — updating with new values.';
    ELSE
        RAISE NOTICE '➕ primaryMenu does not exist — adding new array.';
    END IF;

    IF v_secondary_exist THEN
        RAISE NOTICE '🔄 secondaryMenu exists — updating with new values.';
    ELSE
        RAISE NOTICE '➕ secondaryMenu does not exist — adding new array.';
    END IF;

    IF v_flag_exist THEN
        RAISE NOTICE '🔄 showSelectedPrimaryMenuItem exists — updating value to TRUE.';
    ELSE
        RAISE NOTICE '➕ showSelectedPrimaryMenuItem does not exist — adding flag with value TRUE.';
    END IF;

    -- ✅ Step 4: Update JSON — modify or insert all relevant keys
    UPDATE tenant.tenant
    SET tenant_option_json =
        jsonb_set(
            jsonb_set(
                jsonb_set(
                    COALESCE(tenant_option_json, '{}'::jsonb),
                    '{benefitsOptions,primaryMenu}',
                    COALESCE(v_json_input->'primaryMenu', v_benefits_opts->'primaryMenu', '[]'::jsonb),
                    TRUE
                ),
                '{benefitsOptions,secondaryMenu}',
                COALESCE(v_json_input->'secondaryMenu', v_benefits_opts->'secondaryMenu', '[]'::jsonb),
                TRUE
            ),
            '{benefitsOptions,showSelectedPrimaryMenuItem}',
            COALESCE(v_json_input->'showSelectedPrimaryMenuItem', 'true'::jsonb),
            TRUE
        )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE '✅ Successfully applied menu updates for tenant_code: %', v_tenant_code;

    -- ✅ Optional Step: Log summary
    RAISE NOTICE '📊 Summary: primaryMenu % | secondaryMenu % | showSelectedPrimaryMenuItem %',
        CASE WHEN v_primary_exist THEN 'updated' ELSE 'added' END,
        CASE WHEN v_secondary_exist THEN 'updated' ELSE 'added' END,
        CASE WHEN v_flag_exist THEN 'updated' ELSE 'added' END;

    RAISE NOTICE '🏁 Script execution completed successfully.';
END
$$;
