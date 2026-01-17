-- ============================================================================
-- 🚀 Script    : Rollback UX disableButtton colors Styling from Tenant Attribute for HAP
-- 📌 Purpose   : Removes the "disableButton" object inside "ux" from tenant_attr JSONB
-- 🧑 Author    : Bhojesh
-- 📅 Date      : 2025-11-28
-- 🧾 Jira      : BEN-1278
-- ⚠️ Inputs    : <HAP-TENANT-CODE>
-- 📤 Output    : Removes the "disableButton" object from the JSONB structure
-- 🔗 Script URL: <NA>
-- 📝 Notes     : This script assumes the tenant_attr column is of type JSONB.
--               If "disableButton" does not exist, no changes will be made.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
         'HAP-TENANT-CODE'
    ];
    v_tenant_code TEXT;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        -- Remove disableButton from tenant_attr JSONB
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux}',
            (tenant_attr->'ux')::jsonb - 'disableButton',
            true
        )
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '[Rollback] Removed "disableButton" from tenant: %', v_tenant_code;
    END LOOP;  -- ✅ FIXED
END $$;
 