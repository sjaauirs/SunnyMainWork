-- ===================================================================================
-- Purpose       : Add/Update accordionExpandColor and accordionCollapseColor in tenant_attr JSONB
-- Author        : Rakesh Penati 
-- Description   :
--   - For HAP tenant: sets both colors to #FF7200
--   - For other tenants: sets both colors to #0078b3
--   - Ensures tenant_attr is not NULL or {}
--   - Ensures ux.commonColors exists
-- JIRA Ticket :BEN-17 
-- ===================================================================================

DO $$
DECLARE
    v_hap_tenant_code   TEXT   := '<HAP-TENANT-CODE>';  -- Input HAP tenant code
    v_other_tenants     TEXT[] := ARRAY['<KP-TENANT-CODE>','<WATCO-TENANT-CODE>', '<NAVITUS-TENANT-CODE>'];  -- Input other tenant codes

    v_count INT;
BEGIN
    RAISE NOTICE '--- Starting update of tenant colors ---';

    -- Update HAP tenant
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
        jsonb_set(tenant_attr, '{ux,commonColors,accordionExpandColor}', '"#FF7200"', true),
        '{ux,commonColors,accordionCollapseColor}', '"#FF7200"', true
    )
    WHERE tenant_code = v_hap_tenant_code
      AND delete_nbr = 0
      AND tenant_attr IS NOT NULL
      AND tenant_attr::TEXT <> '{}'
      AND tenant_attr ? 'ux'
      AND (tenant_attr->'ux') ? 'commonColors';

    GET DIAGNOSTICS v_count = ROW_COUNT;
    RAISE NOTICE 'Updated HAP tenant % colors (rows=%).', v_hap_tenant_code, v_count;

    -- Update other tenants
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
        jsonb_set(tenant_attr, '{ux,commonColors,accordionExpandColor}', '"#0078b3"', true),
        '{ux,commonColors,accordionCollapseColor}', '"#0078b3"', true
    )
    WHERE tenant_code = ANY(v_other_tenants)
      AND delete_nbr = 0
      AND tenant_attr IS NOT NULL
      AND tenant_attr::TEXT <> '{}'
      AND tenant_attr ? 'ux'
      AND (tenant_attr->'ux') ? 'commonColors';

    GET DIAGNOSTICS v_count = ROW_COUNT;
    RAISE NOTICE 'Updated OTHER tenants % colors (rows=%).', v_count, v_other_tenants;

    RAISE NOTICE '--- Tenant color update completed successfully ---';
EXCEPTION
    WHEN OTHERS THEN
        RAISE WARNING 'Error while updating tenant colors: %', SQLERRM;
        RAISE;
END $$;
