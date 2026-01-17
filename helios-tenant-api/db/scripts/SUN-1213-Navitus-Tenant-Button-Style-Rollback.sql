-- ============================================================================
-- 🚀 Script    : Rollback UX button Styling from Tenant Attribute for NAVITUS
-- 📌 Purpose   : Removes the "button" object inside "ux" from tenant_attr JSONB
-- 🧑 Author    : Bhojesh
-- 📅 Date      : 2025-12-01
-- 🧾 Jira      : SUN-1213
-- ⚠️ Inputs    : <NAVITUS-TENANT-CODE>
-- 📤 Output    : Removes the "button" object from the JSONB structure
-- 🔗 Script URL: <NA>
-- 📝 Notes     : This script assumes the tenant_attr column is of type JSONB.
--               If "button" does not exist, no changes will be made.
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
        -- Remove button from tenant_attr JSONB
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux}',
            (tenant_attr->'ux')::jsonb - 'button',
            true
        )
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '[Rollback] Removed "button" from tenant: %', v_tenant_code;
    END LOOP;  -- ✅ FIXED
END $$;
 