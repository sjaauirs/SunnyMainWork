-- ==================================================================================================
-- 🚀 Rollback Script : Remove Menus and Flag from tenant_option_json → benefitsOptions
-- 📌 Purpose         : Delete only `primaryMenu`, `secondaryMenu`, and 
--                      `showSelectedPrimaryMenuItem` keys under benefitsOptions.
-- 🧑 Author          : Siva Krishna Reddy
-- 📅 Date            : 2025-10-20
-- 🧾 Jira            : RES-898
-- ⚠️ Inputs          : SUNNY-TENANT-CODE
-- 📤 Output          : Removes specified keys only; preserves all other JSON content.
-- 🔗 Script URL      : NA
-- 📝 Notes           :
--   - Does not remove other keys inside benefitsOptions.
--   - Safe to run multiple times (idempotent).
--   - If benefitsOptions does not exist or keys already removed, logs info message.
-- ==================================================================================================

DO
$$
DECLARE
    v_tenant_code   TEXT := '<SUNNY-TENANT-CODE>';  -- Replace with target tenant code
    v_option_json   JSONB;
    v_benefits_opts JSONB;
    v_exists        BOOLEAN := FALSE;
BEGIN
    RAISE NOTICE '🔍 Starting rollback for tenant_code: %', v_tenant_code;

    -- ✅ Step 1: Validate tenant existence
    SELECT tenant_option_json INTO v_option_json
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0
    LIMIT 1;

    IF NOT FOUND THEN
        RAISE NOTICE '⚠️ No active tenant found for tenant_code: %', v_tenant_code;
        RETURN;
    END IF;

    -- ✅ Step 2: Extract benefitsOptions if it exists
    v_benefits_opts := v_option_json -> 'benefitsOptions';
    IF v_benefits_opts IS NULL THEN
        RAISE NOTICE 'ℹ️ No benefitsOptions node found — nothing to rollback.';
        RETURN;
    END IF;

    -- ✅ Step 3: Check if any of the target keys exist
    v_exists := (
        (v_benefits_opts ? 'primaryMenu') OR
        (v_benefits_opts ? 'secondaryMenu') OR
        (v_benefits_opts ? 'showSelectedPrimaryMenuItem')
    );

    IF NOT v_exists THEN
        RAISE NOTICE 'ℹ️ No target keys found under benefitsOptions — nothing to rollback.';
        RETURN;
    END IF;

    -- ✅ Step 4: Remove only the specified keys under benefitsOptions
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json,
        '{benefitsOptions}',
        (tenant_option_json -> 'benefitsOptions')
            - 'primaryMenu'
            - 'secondaryMenu'
            - 'showSelectedPrimaryMenuItem',
        TRUE
    )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE '♻️ Removed primaryMenu, secondaryMenu, and showSelectedPrimaryMenuItem under benefitsOptions for tenant: %',
        v_tenant_code;

    RAISE NOTICE '🏁 Rollback completed successfully for tenant_code: %', v_tenant_code;
END
$$;
