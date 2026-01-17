-- ============================================================================
-- 🚀 Script    : Script to Update flag "enableSelfAttestationCompletion" in tenant_attr column for given tenants
-- 📌 Purpose   : Update specific tenants with the flag for UI behavior
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 03-11-2025
-- 🧾 Jira      : RES-1027(subtask)
-- ⚠️ Inputs    : Array of tenant codes (update only those tenants)
-- 📤 Output    : Adds/updates the flag "enableSelfAttestationCompletion"=true in tenant_attr for given tenants
-- 🔗 Script URL: NA
-- 📝 Notes     : Only affects tenants provided in the array
-- ============================================================================

DO $$
DECLARE

    -- Input array: Update this list of tenant codes as needed
    v_tenant_codes TEXT[] := ARRAY[
        '<HAP-TENANT-CODE>',
        '<HAP-TENANT-CODE>'
    ];
    v_tenant RECORD;
    v_updated_count INT := 0;
    v_processed_count INT := 0;
    v_skipped_count INT := 0;
    v_exists BOOLEAN;
    v_rowcount INT := 0;


BEGIN
    RAISE NOTICE '[Information] Starting targeted update of "enableSelfAttestationCompletion" flag for selected tenants...';

    FOR v_tenant IN
        SELECT tenant_code, tenant_attr
        FROM tenant.tenant
        WHERE delete_nbr = 0
          AND tenant_code = ANY(v_tenant_codes)
    LOOP
        v_processed_count := v_processed_count + 1;

        SELECT (v_tenant.tenant_attr ? 'enableSelfAttestationCompletion')
        INTO v_exists;

        IF v_exists THEN
            -- Flag exists, ensure it's TRUE
            UPDATE tenant.tenant
            SET tenant_attr = tenant_attr || jsonb_build_object('enableSelfAttestationCompletion', true)
            WHERE tenant_code = v_tenant.tenant_code
              AND delete_nbr = 0;

            GET DIAGNOSTICS v_rowcount = ROW_COUNT;
            v_updated_count := v_updated_count + v_rowcount;

            RAISE NOTICE '[Updated] Tenant % - Flag existed; set to TRUE.', v_tenant.tenant_code;
        ELSE
            -- Flag missing, add with TRUE
            UPDATE tenant.tenant
            SET tenant_attr = tenant_attr || jsonb_build_object('enableSelfAttestationCompletion', true)
            WHERE tenant_code = v_tenant.tenant_code
              AND delete_nbr = 0;

            GET DIAGNOSTICS v_rowcount = ROW_COUNT;
            v_updated_count := v_updated_count + v_rowcount;

            RAISE NOTICE '[Added] Tenant % - Flag "enableSelfAttestationCompletion" added with TRUE.', v_tenant.tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE '[Summary] Processed tenants : %', v_processed_count;
    RAISE NOTICE '[Summary] Updated tenants   : %', v_updated_count;
    RAISE NOTICE '[Summary] Skipped tenants   : %', v_skipped_count;
    RAISE NOTICE '[Information] Completed targeted update successfully.';

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '[Error] Unexpected error during update: %', SQLERRM;
        RAISE;
END $$;
