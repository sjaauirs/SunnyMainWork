-- ============================================================================
-- 🚀 Script    : Rollback - Remove subscriptionStatus from subscription_status_json
-- 📌 Purpose   : 
--   1. Removes the 'subscriptionStatus' node for all consumers under the given tenant.
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-10-28
-- 🧾 Jira      : RES-725
-- ⚙️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : Removes subscriptionStatus key from huser.consumer.subscription_status_json
-- 📝 Notes     :
--   - Idempotent and safe to re-run.
--   - Does not affect other JSON keys in subscription_status_json.
--   - Applies only to records with delete_nbr = 0.
-- ============================================================================

DO
$$
DECLARE
    v_input_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- 🔸 Input tenant code here
    v_removed_count INTEGER := 0;
BEGIN
    RAISE NOTICE '🚀 Starting rollback: removing subscriptionStatus from subscription_status_json for tenant_code = %', v_input_tenant_code;

    -- 🔹 Remove only the 'subscriptionStatus' key from subscription_status_json
    UPDATE huser.consumer
    SET subscription_status_json = subscription_status_json - 'subscriptionStatus'
    WHERE tenant_code = v_input_tenant_code
      AND delete_nbr = 0
      AND subscription_status_json ? 'subscriptionStatus';

    GET DIAGNOSTICS v_removed_count = ROW_COUNT;

    -- 🔹 Logging summary
    RAISE NOTICE '♻️ % consumer(s) rolled back (subscriptionStatus removed) for tenant_code = %.', v_removed_count, v_input_tenant_code;
    RAISE NOTICE '🎯 Rollback completed successfully.';
END
$$;
