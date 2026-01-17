-- ===================================================================================
-- Purpose       : Remove accordionExpandColor and accordionCollapseColor from tenant_attr JSONB
-- Author        : Rakesh Pernati 
-- Description   :
--   - Removes the two keys from ux.commonColors
--   - Keeps all other JSON structure intact
-- JIRA Ticket :BEN-17 
-- ===================================================================================

DO $$
DECLARE
    v_hap_tenant_code   TEXT   := '<HAP-TENANT-CODE>';  -- Input HAP tenant code
    v_other_tenants     TEXT[] := ARRAY['<KP-TENANT-CODE>','<WATCO-TENANT-CODE>', '<NAVITUS-TENANT-CODE>'];  -- Input other tenant codes

    v_count INT;
BEGIN
    RAISE NOTICE '--- Starting rollback of tenant colors ---';

    -- Rollback HAP tenant
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
        tenant_attr,
        '{ux,commonColors}',
        ((tenant_attr->'ux'->'commonColors') - 'accordionExpandColor' - 'accordionCollapseColor'),
        true
    )
    WHERE tenant_code = v_hap_tenant_code
      AND delete_nbr = 0
      AND tenant_attr IS NOT NULL
      AND tenant_attr::TEXT <> '{}'
      AND tenant_attr ? 'ux'
      AND (tenant_attr->'ux') ? 'commonColors';

    GET DIAGNOSTICS v_count = ROW_COUNT;
    RAISE NOTICE 'Rolled back HAP tenant % colors (rows=%).', v_hap_tenant_code, v_count;

    -- Rollback other tenants
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
        tenant_attr,
        '{ux,commonColors}',
        ((tenant_attr->'ux'->'commonColors') - 'accordionExpandColor' - 'accordionCollapseColor'),
        true
    )
    WHERE tenant_code = ANY(v_other_tenants)
      AND delete_nbr = 0
      AND tenant_attr IS NOT NULL
      AND tenant_attr::TEXT <> '{}'
      AND tenant_attr ? 'ux'
      AND (tenant_attr->'ux') ? 'commonColors';

    GET DIAGNOSTICS v_count = ROW_COUNT;
    RAISE NOTICE 'Rolled back OTHER tenants % colors (rows=%).', v_other_tenants, v_count;

    RAISE NOTICE '--- Tenant color rollback completed successfully ---';
EXCEPTION
    WHEN OTHERS THEN
        RAISE WARNING 'Error while rolling back tenant colors: %', SQLERRM;
        RAISE;
END $$;
