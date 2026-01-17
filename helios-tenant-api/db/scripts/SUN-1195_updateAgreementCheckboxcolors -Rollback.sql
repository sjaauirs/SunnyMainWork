-- ============================================================================
-- 🚀 Script    : Revert checkbox agreementColors in tenant_attr JSONB
-- 📌 Purpose   : Ensures tenant_attr.ux.agreementColors has the correct palette
-- 🧑 Author    : Neel
-- 📅 Date      : 2025-11-20
-- 🧾 Jira      : SUN-1195
-- ⚠️ Inputs    : v_tenant_code (Tenant identifier) HAP only
-- 📤 Output    : Revert agreementColors JSONB with enforced values
-- 📝 Notes     : Idempotent, always resets to the expected palette
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- Input tenant code
    v_old_attr JSONB;
BEGIN
    -- Fetch current tenant_attr
    SELECT tenant_attr
	INTO v_old_attr
      FROM tenant.tenant
     WHERE tenant_code = v_tenant_code
       AND delete_nbr = 0
       AND tenant_attr IS NOT NULL
       AND tenant_attr::text <> '{}'
       AND ( (tenant_attr->'ux') ? 'agreementColors');

    IF NOT FOUND THEN
        RAISE WARNING 'No tenant found or tenant_attr is empty for tenant_code: %', v_tenant_code;
        RETURN;
    END IF;

        UPDATE tenant.tenant
            SET tenant_attr = tenant_attr - '{ux,agreementColors,agreeCheckboxColor}'
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE 'tenant_attr.ux.agreementColors agreeCheckboxColor reverted successfully for tenant %', v_tenant_code;
   
END $$;
