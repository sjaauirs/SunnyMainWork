-- ============================================================================
-- 🚀 Script    : Add/Update requiredAsteriskColor in tenant_attr JSONB
-- 📌 Purpose   : Ensures tenant_attr contains ux.commonColors.requiredAsteriskColor. 
--               Adds the key with default value (#D43211) if missing, otherwise skips.
-- 🧑 Author    : Rakesh Pernati
-- 📅 Date      : 19-09-2025
-- 🧾 Jira      : BEN-599
-- ⚠️ Inputs    : v_tenant_code (Tenant code for which update needs to be applied)
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

    -- Add/Update radioActiveBgColor
    IF (v_new_attr #>> '{ux,commonColors,requiredAsteriskColor}') IS NULL THEN
        v_new_attr := jsonb_set(v_new_attr, '{ux,commonColors,requiredAsteriskColor}', to_jsonb('#D43211'::text), true);
        v_updated := true;
        RAISE NOTICE 'requiredAsteriskColor added for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'requiredAsteriskColor already exists for tenant %', v_tenant_code;
    END IF;

    
    -- Update only if something changed
    IF v_updated THEN
        UPDATE tenant.tenant
        SET tenant_attr = v_new_attr
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE 'tenant_attr updated successfully for tenant %', v_tenant_code;
    ELSE
        RAISE WARNING 'No changes made, key is already exist for tenant %', v_tenant_code;
    END IF;
END $$;
