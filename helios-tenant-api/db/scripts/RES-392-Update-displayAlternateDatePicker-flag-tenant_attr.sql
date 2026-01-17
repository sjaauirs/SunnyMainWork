-- ============================================================================
-- 🚀 Script    : Script to Update flag "displayAlternateDatePicker" in tenant_attr column with default value TRUE
-- 📌 Purpose   : Based on this flag UI will flip the dialer in UI
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 29-09-2025
-- 🧾 Jira      : RES-392 & RES-561(Sub-task)
-- ⚠️ Inputs    : HAP-TENANT-CODE
-- 📤 Output    : It will Update the flag "displayAlternateDatePicker" to 'true' for input tenant in tenant.tenant
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- <<<< Replace with actual tenant_code
    v_updated_count INT := 0;
    v_exists BOOLEAN;
BEGIN
    RAISE NOTICE '[Information] Starting update: Attempting to set "displayAlternateDatePicker"=true for tenant_code=%', v_tenant_code;

    -- Check if flag exists for this tenant
    SELECT (tenant_attr ? 'displayAlternateDatePicker')
    INTO v_exists
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    IF v_exists IS DISTINCT FROM TRUE THEN
        RAISE NOTICE '[Error] The flag "displayAlternateDatePicker" does not exist for tenant_code=%', v_tenant_code;
        RETURN; -- exit without updating
    END IF;

    -- Update the flag to true
    UPDATE tenant.tenant t
    SET tenant_attr = tenant_attr || jsonb_build_object('displayAlternateDatePicker', true)
    WHERE t.delete_nbr = 0
      AND t.tenant_code = v_tenant_code;

    GET DIAGNOSTICS v_updated_count = ROW_COUNT;

    IF v_updated_count > 0 THEN
        RAISE NOTICE '[Information] Successfully updated "displayAlternateDatePicker"=true for tenant_code=%', v_tenant_code;
    ELSE
        RAISE NOTICE '[Error] No rows updated for tenant_code=% (possible data issue)', v_tenant_code;
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '[Error] Unexpected error while updating tenant_code=% : %', v_tenant_code, SQLERRM;
        RAISE;
END $$;
