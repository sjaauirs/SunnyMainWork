-- ============================================================================
-- 🚀 Script    : Rollback tenant_option_json → benefitsOptions.hamburgerMenu
-- 📌 Purpose   : Rollback `hamburgerMenu` array inside tenant_option_json.benefitsOptions
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-10-22
-- 🧾 Jira      : RES-922
-- ⚠️ Inputs    : SUNNY-TENANT-CODE
-- 📤 Output    : Rollback JSON structure; logs status messages via RAISE NOTICE
-- 🔗 Script URL: NA
-- 📝 Notes     : 
--   - Preserves existing keys under benefitsOptions.
--   - Adds `hamburgerMenu` if not present, otherwise updates with new values.
-- ============================================================================

DO
$$
DECLARE
    v_tenant_code   TEXT := '<SUNNY-TENANT-CODE>';  -- Replace with target tenant code
    v_json_input    JSONB := '{
        "hamburgerMenu": [
            "myCard",
            "myRewards",
            "personal",
            "manageCard",
            "privacyPolicy",
            "signOut"
        ]
    }'::jsonb;

    v_option_json   JSONB;
    v_benefits_opts JSONB;
    v_hamburger_exist BOOLEAN := FALSE;
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

    RAISE NOTICE '✅ Tenant found. Proceeding with hamburgerMenu update.';

    -- ✅ Step 2: Extract existing benefitsOptions if available
    v_benefits_opts := COALESCE(v_option_json->'benefitsOptions', '{}'::jsonb);

    -- ✅ Step 3: Check if hamburgerMenu already exists
    v_hamburger_exist := v_benefits_opts ? 'hamburgerMenu';

    IF v_hamburger_exist THEN
        RAISE NOTICE '🔄 hamburgerMenu exists — updating with new values.';
    ELSE
        RAISE NOTICE '➕ hamburgerMenu does not exist — adding new array.';
    END IF;

    -- ✅ Step 4: Rollback JSON — modify or insert the hamburgerMenu key
    UPDATE tenant.tenant
    SET tenant_option_json =
        jsonb_set(
            COALESCE(tenant_option_json, '{}'::jsonb),
            '{benefitsOptions,hamburgerMenu}',
            COALESCE(v_json_input->'hamburgerMenu', v_benefits_opts->'hamburgerMenu', '[]'::jsonb),
            TRUE
        )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE '✅ Successfully applied hamburgerMenu Rollback for tenant_code: %', v_tenant_code;

    -- ✅ Optional Step: Log summary
    RAISE NOTICE '📊 Summary: hamburgerMenu %',
        CASE WHEN v_hamburger_exist THEN 'updated' ELSE 'added' END;

    RAISE NOTICE '🏁 Script execution completed successfully.';
END
$$;
