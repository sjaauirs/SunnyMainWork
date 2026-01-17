-- ===================================================================================
-- Purpose       : Rollback - Remove disableRewardsUntilProgramStart from tenant_attr JSONB
-- Author        : Rakesh Pernati 
-- JIRA Ticket   : BEN-581
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

    -- Remove disableRewardsUntilProgramStart key if it exists
    IF (v_new_attr ? 'disableRewardsUntilProgramStart') THEN
        v_new_attr := v_new_attr - 'disableRewardsUntilProgramStart';
        v_updated := true;
        RAISE NOTICE 'disableRewardsUntilProgramStart flag removed for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'disableRewardsUntilProgramStart flag not found for tenant %', v_tenant_code;
    END IF;

    -- Update only if something changed
    IF v_updated THEN
        UPDATE tenant.tenant
        SET tenant_attr = v_new_attr
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE 'tenant_attr rollback applied successfully for tenant %', v_tenant_code;
    ELSE
        RAISE WARNING 'No rollback changes made for tenant %', v_tenant_code;
    END IF;
END $$;
