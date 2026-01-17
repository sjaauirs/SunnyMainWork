-- ============================================================================
-- 🚀 Script    : Script to Update flag "displayAlternateDialer" in tenant_attr column with default value False
-- 📌 Purpose   : Rollback the flag to default value False
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 24-09-2025
-- 🧾 Jira      : RES-150 & RES-560(Sub-task)
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : It will Rollback(update to default value) the flag "displayAlternateDialer" to 'false' for input tenant in tenant.tenant
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- <<<< Replace with actual tenant_code
    v_updated_count INT := 0;
    v_exists BOOLEAN;
BEGIN
    RAISE NOTICE '[Information] Starting rollback: Attempting to set "displayAlternateDialer"=false for tenant_code=%', v_tenant_code;

    -- Check if flag exists for this tenant
    SELECT (tenant_attr ? 'displayAlternateDialer')
    INTO v_exists
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    IF v_exists IS DISTINCT FROM TRUE THEN
        RAISE NOTICE '[Error] The flag "displayAlternateDialer" does not exist for tenant_code=%', v_tenant_code;
        RETURN; -- exit without updating
    END IF;

    -- Update the flag back to false (Rollback)
    UPDATE tenant.tenant t
    SET tenant_attr = tenant_attr || jsonb_build_object('displayAlternateDialer', false)
    WHERE t.delete_nbr = 0
      AND t.tenant_code = v_tenant_code;

    GET DIAGNOSTICS v_updated_count = ROW_COUNT;

    IF v_updated_count > 0 THEN
        RAISE NOTICE '[Information] Successfully rolled back: "displayAlternateDialer"=false for tenant_code=%', v_tenant_code;
    ELSE
        RAISE NOTICE '[Error] No rows updated for tenant_code=% (possible data issue)', v_tenant_code;
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '[Error] Unexpected error while rolling back tenant_code=% : %', v_tenant_code, SQLERRM;
        RAISE;
END $$;

