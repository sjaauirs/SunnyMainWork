-- ============================================================================
-- 🚀 Script    : Update subscriptionStatus in tenant_option_json (per tenant)
-- 📌 Purpose   : 
--   1. For the given HAP tenant_code → Set subscriptionStatus.status = 'not_subscribed'
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-10-27
-- 🧾 Jira      : RES-725
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : Updates tenant_option_json.subscriptionStatus accordingly.
-- 🔗 Script URL: N/A
-- 📝 Notes     :
--   - Idempotent and safe to re-run.
--   - Preserves existing tenant_option_json content.
-- ============================================================================

DO
$$
DECLARE
    v_input_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- 🔸 Input tenant code here
    v_updated_not_subscribed_count INTEGER := 0;
BEGIN
    RAISE NOTICE '🚀 Starting subscriptionStatus update process for tenant_code = %', v_input_tenant_code;

    -- 🔹 1. Update the input tenant → not_subscribed
    UPDATE tenant.tenant
    SET tenant_option_json = 
        jsonb_set(
            COALESCE(tenant_option_json, '{}'::jsonb),
            '{subscriptionStatus}',
            jsonb_build_array(
                jsonb_build_object(
                    'feature', 'myRewards',
                    'status', 'not_subscribed'
                )
            ),
            TRUE
        )
    WHERE delete_nbr = 0
      AND tenant_code = v_input_tenant_code;

    GET DIAGNOSTICS v_updated_not_subscribed_count = ROW_COUNT;


    -- 🔹 Logging summary
    RAISE NOTICE '✅ % tenant(s) updated to "not_subscribed" (tenant_code = %).', v_updated_not_subscribed_count, v_input_tenant_code;
END
$$;
