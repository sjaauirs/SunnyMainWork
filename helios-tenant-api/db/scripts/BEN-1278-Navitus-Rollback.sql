-- ============================================================================
-- 🚀 Script    : Rollback UX shopColors Styling from Tenant Attribute for NAVITUS
-- 📌 Purpose   : Removes the "shopColors" object inside "ux" from tenant_attr JSONB
-- 🧑 Author    : Bhojesh
-- 📅 Date      : 2025-10-09
-- 🧾 Jira      : BEN-1278
-- ⚠️ Inputs    : <NAVITUS-TENANT-CODE>
-- 📤 Output    : Removes the "shopColors" object from the JSONB structure
-- 🔗 Script URL: <NA>
-- 📝 Notes     : This script assumes the tenant_attr column is of type JSONB.
--               If "shopColors" does not exist, no changes will be made.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
         'NAVITUS-TENANT-CODE'
    ];
    v_tenant_code TEXT;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        -- Remove shopColors from tenant_attr JSONB
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux}',
            (tenant_attr->'ux')::jsonb - 'shopColors',
            true
        )
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '[Rollback] Removed "shopColors" from tenant: %', v_tenant_code;
    END LOOP;  -- ✅ FIXED
END $$;
 