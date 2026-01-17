-- ===================================================================================
-- Purpose       : Rollback - Remove headerBgColor from tenant_attr JSONB
-- Author        : Rakesh Pernati 
-- JIRA Ticket   : BEN-582
-- ===================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- Input tenant code
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

    -- Remove headerBgColor if it exists
    IF (v_new_attr #>> '{ux,headerColors,headerBgColor}') IS NOT NULL THEN
        v_new_attr := v_new_attr #- '{ux,headerColors,headerBgColor}';
        v_updated := true;
        RAISE NOTICE 'headerBgColor removed from tenant_attr for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'headerBgColor not found in tenant_attr for tenant % (no change)', v_tenant_code;
    END IF;

    -- Update only if something changed
    IF v_updated THEN
        UPDATE tenant.tenant
        SET tenant_attr = v_new_attr
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE 'tenant_attr rollback applied successfully for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'No rollback changes made for tenant %', v_tenant_code;
    END IF;
END $$;
