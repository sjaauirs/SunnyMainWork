-- ===================================================================================
-- Purpose       : Insert/Update headerBgColor in tenant_attr JSONB
-- Author        : Rakesh Pernati 
-- JIRA Ticket   : BEN-582
-- ===================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- Input tenant code
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_updated BOOLEAN := false;
    v_new_color TEXT := '#FBF8F6'; -- Desired header background color
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

    -- Insert if not exists OR update if value differs
    IF (v_new_attr #>> '{ux,headerColors,headerBgColor}') IS DISTINCT FROM v_new_color THEN
        v_new_attr := jsonb_set(v_new_attr, '{ux,headerColors,headerBgColor}', to_jsonb(v_new_color), true);
        v_updated := true;

        IF (v_old_attr #>> '{ux,headerColors,headerBgColor}') IS NULL THEN
            RAISE NOTICE 'headerBgColor inserted with value % for tenant %', v_new_color, v_tenant_code;
        ELSE
            RAISE NOTICE 'headerBgColor updated to value % for tenant %', v_new_color, v_tenant_code;
        END IF;
    ELSE
        RAISE NOTICE 'headerBgColor already set to % for tenant % (no change)', v_new_color, v_tenant_code;
    END IF;

    -- Update only if something changed
    IF v_updated THEN
        UPDATE tenant.tenant
        SET tenant_attr = v_new_attr
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE 'tenant_attr updated successfully for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'No changes made, tenant_attr already has correct value for tenant %', v_tenant_code;
    END IF;
END $$;
