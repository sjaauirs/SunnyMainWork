-- ============================================================================
-- 🚀 Script    : Update subscription_status_json in huser.consumer
-- 📌 Purpose   : 
--   1. For given tenant_code, update subscription_status_json based on onboarding_state.
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-10-28
-- 🧾 Jira      : RES-725
-- ⚙️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : Updates huser.consumer.subscription_status_json accordingly.
-- 📝 Notes     :
--   - onboarding_state = 'VERIFIED' → status = 'subscribed'
--   - onboarding_state != 'VERIFIED' → status = 'not_subscribed'
--   - Idempotent and safe to re-run.
--   - Preserves other consumer columns.
-- ============================================================================

DO
$$
DECLARE
    v_input_tenant_code TEXT := '<HAP-TENANT-CODE>';   -- 🔸 Input tenant code here

    v_verified_state TEXT := 'VERIFIED';
    v_updated_subscribed_count INTEGER := 0;
    v_updated_not_subscribed_count INTEGER := 0;
BEGIN
    RAISE NOTICE '🚀 Starting subscription_status_json update process for tenant_code = %', v_input_tenant_code;

    -- 🔹 1. Update consumers with onboarding_state = 'verified'
    UPDATE huser.consumer
    SET subscription_status_json = jsonb_build_object(
        'subscriptionStatus',
        jsonb_build_array(
            jsonb_build_object(
                'status', 'subscribed',
                'feature', 'myRewards'
            )
        )
    )
    WHERE tenant_code = v_input_tenant_code
      AND onboarding_state = v_verified_state
      AND delete_nbr = 0;

    GET DIAGNOSTICS v_updated_subscribed_count = ROW_COUNT;

    -- 🔹 2. Update consumers where onboarding_state != 'verified'
    UPDATE huser.consumer
    SET subscription_status_json = jsonb_build_object(
        'subscriptionStatus',
        jsonb_build_array(
            jsonb_build_object(
                'status', 'not_subscribed',
                'feature', 'myRewards'
            )
        )
    )
    WHERE tenant_code = v_input_tenant_code
      AND onboarding_state <> v_verified_state
      AND delete_nbr = 0;

    GET DIAGNOSTICS v_updated_not_subscribed_count = ROW_COUNT;

    -- 🔹 Logging summary
    RAISE NOTICE '✅ % consumer(s) set to "subscribed" (onboarding_state = verified)', v_updated_subscribed_count;
    RAISE NOTICE '✅ % consumer(s) set to "not_subscribed" (onboarding_state != verified)', v_updated_not_subscribed_count;
    RAISE NOTICE '🎯 Subscription status update completed for tenant_code = %', v_input_tenant_code;
END
$$;
