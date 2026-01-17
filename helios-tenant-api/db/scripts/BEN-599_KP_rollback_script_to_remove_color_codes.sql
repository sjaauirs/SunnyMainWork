-- ============================================================================
-- 🚀 Script    : Rollback - Remove requiredAsteriskColor from tenant_attr JSONB
-- 📌 Purpose   : Removes ux.commonColors.requiredAsteriskColor if present.
-- 🧑 Author    : Rakesh Pernati
-- 📅 Date      : 19-09-2025
-- 🧾 Jira      : BEN-599
-- ⚠️ Inputs    : v_tenant_code (Tenant code for which rollback needs to be applied)
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- Input tenant code
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_updated BOOLEAN := false;
BEGIN
    -- Fetch current tenant_attr
    SELECT tenant_attr
    INTO v_old_attr
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0
      AND tenant_attr IS NOT NULL
      AND tenant_attr::text <> '{}';

    IF NOT FOUND THEN
        RAISE WARNING 'No tenant found or tenant_attr is empty for tenant_code: %', v_tenant_code;
        RETURN;
    END IF;

    v_new_attr := v_old_attr;

    -- Remove requiredAsterickColor if exists
    IF (v_new_attr #>> '{ux,commonColors,requiredAsteriskColor}') IS NOT NULL THEN
        v_new_attr := v_new_attr #- '{ux,commonColors,requiredAsteriskColor}';
        v_updated := true;
        RAISE NOTICE 'requiredAsteriskColor removed for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'requiredAsteriskColor not found for tenant %, no rollback needed', v_tenant_code;
    END IF;

    -- Update only if something changed
    IF v_updated THEN
        UPDATE tenant.tenant
        SET tenant_attr = v_new_attr
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE 'Rollback successful, tenant_attr updated for tenant %', v_tenant_code;
    END IF;
END $$;
