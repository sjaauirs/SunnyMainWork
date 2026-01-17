-- ============================================================================
-- 🚀 Script    : Script to Update flag "enableSelfAttestationCompletion" in tenant_attr column with default value FALSE
-- 📌 Purpose   : Update all active tenants with the flag set to FALSE for UI behavior
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 03-11-2025
-- 🧾 Jira      : RES-1027(subtask)
-- ⚠️ Inputs    : None (applies to all tenants with delete_nbr = 0)
-- 📤 Output    : Adds/updates the flag "enableSelfAttestationCompletion"=false in tenant_attr for all active tenants
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ============================================================================

DO $$
DECLARE
    v_tenant RECORD;
    v_updated_count INT := 0;
    v_processed_count INT := 0;
    v_skipped_count INT := 0;
    v_exists BOOLEAN;
    v_rowcount INT := 0;
BEGIN
    RAISE NOTICE '[Information] Starting bulk update of "enableSelfAttestationCompletion" flag to FALSE for all active tenants...';

    FOR v_tenant IN
        SELECT tenant_code, tenant_attr
        FROM tenant.tenant
        WHERE delete_nbr = 0
    LOOP
        v_processed_count := v_processed_count + 1;

        SELECT (v_tenant.tenant_attr ? 'enableSelfAttestationCompletion')
        INTO v_exists;

        IF v_exists THEN
            -- Flag already exists, just ensure it’s set to FALSE
            UPDATE tenant.tenant
            SET tenant_attr = tenant_attr || jsonb_build_object('enableSelfAttestationCompletion', false)
            WHERE tenant_code = v_tenant.tenant_code
              AND delete_nbr = 0;

            GET DIAGNOSTICS v_rowcount = ROW_COUNT;
            v_updated_count := v_updated_count + v_rowcount;

            RAISE NOTICE '[Updated] Tenant % - Flag already existed; set to FALSE.', v_tenant.tenant_code;
        ELSE
            -- Flag does not exist, add it as FALSE
            UPDATE tenant.tenant
            SET tenant_attr = tenant_attr || jsonb_build_object('enableSelfAttestationCompletion', false)
            WHERE tenant_code = v_tenant.tenant_code
              AND delete_nbr = 0;

            GET DIAGNOSTICS v_rowcount = ROW_COUNT;
            v_updated_count := v_updated_count + v_rowcount;

            RAISE NOTICE '[Added] Tenant % - Flag "enableSelfAttestationCompletion" added with FALSE.', v_tenant.tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE '[Summary] Processed tenants : %', v_processed_count;
    RAISE NOTICE '[Summary] Updated tenants   : %', v_updated_count;
    RAISE NOTICE '[Summary] Skipped tenants   : %', v_skipped_count;
    RAISE NOTICE '[Information] Completed bulk update successfully.';

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '[Error] Unexpected error during bulk update: %', SQLERRM;
        RAISE;
END $$;
