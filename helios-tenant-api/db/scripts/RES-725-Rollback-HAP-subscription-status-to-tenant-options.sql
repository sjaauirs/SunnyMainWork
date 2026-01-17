-- ============================================================================
-- 🚀 Script    : Rollback - Remove subscriptionStatus from tenant_option_json
-- 📌 Purpose   : 
--   1. Removes only the 'subscriptionStatus' node added in the earlier update.
--   2. Safe to re-run (idempotent).
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-10-27
-- 🧾 Jira      : RES-725
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : Removes tenant_option_json.subscriptionStatus for given tenant.
-- 🔗 Script URL: N/A
-- 📝 Notes     :
--   - Does not affect other tenant_option_json keys.
--   - Use only for rollback of the update script.
-- ============================================================================

DO
$$
DECLARE
    v_input_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- 🔸 Input tenant code here
    v_removed_count INTEGER := 0;
BEGIN
    RAISE NOTICE '🚀 Starting rollback: removing subscriptionStatus for tenant_code = %', v_input_tenant_code;

    -- 🔹 Remove only the 'subscriptionStatus' key
    UPDATE tenant.tenant
    SET tenant_option_json = tenant_option_json - 'subscriptionStatus'
    WHERE delete_nbr = 0
      AND tenant_code = v_input_tenant_code
      AND tenant_option_json ? 'subscriptionStatus';

    GET DIAGNOSTICS v_removed_count = ROW_COUNT;

    -- 🔹 Logging summary
    RAISE NOTICE '♻️ % tenant(s) rolled back (subscriptionStatus removed) for tenant_code = %.', v_removed_count, v_input_tenant_code;
END
$$;
