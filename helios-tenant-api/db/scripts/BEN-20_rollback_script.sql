-- ===================================================================================
-- Purpose       : Remove radioActiveBgColor,onboardingSurvey and radioInActiveBgColor from tenant_attr JSONB
-- Author        : Rakesh Pernati 
-- Description   :
--   - Removes the two keys from ux.commonColors
--   - Keeps all other JSON structure intact
-- JIRA Ticket : BEN-20
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

    -- Remove radioActiveBgColor
    IF (v_new_attr #>> '{ux,commonColors,radioActiveBgColor}') IS NOT NULL THEN
        v_new_attr := v_new_attr #- '{ux,commonColors,radioActiveBgColor}';
        v_updated := true;
        RAISE NOTICE 'radioActiveBgColor removed for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'radioActiveBgColor not present for tenant %, nothing to rollback', v_tenant_code;
    END IF;

    -- Remove radioInActiveBgColor
    IF (v_new_attr #>> '{ux,commonColors,radioInActiveBgColor}') IS NOT NULL THEN
        v_new_attr := v_new_attr #- '{ux,commonColors,radioInActiveBgColor}';
        v_updated := true;
        RAISE NOTICE 'radioInActiveBgColor removed for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'radioInActiveBgColor not present for tenant %, nothing to rollback', v_tenant_code;
    END IF;

    -- Remove onboardingSurvey
    IF (v_new_attr -> 'onboardingSurvey') IS NOT NULL THEN
        v_new_attr := v_new_attr - 'onboardingSurvey';
        v_updated := true;
        RAISE NOTICE 'onboardingSurvey removed for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'onboardingSurvey not present for tenant %, nothing to rollback', v_tenant_code;
    END IF;

    -- Update only if something changed
    IF v_updated THEN
        UPDATE tenant.tenant
        SET tenant_attr = v_new_attr
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE 'Rollback completed successfully for tenant %', v_tenant_code;
    ELSE
        RAISE WARNING 'No rollback needed, keys not present for tenant %', v_tenant_code;
    END IF;
END $$;
