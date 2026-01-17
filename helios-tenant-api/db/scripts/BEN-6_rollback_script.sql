-- =====================================================================
-- Script : Rollback tenant_attr changes (remove agreementsColors and agreementDeclineImageUrl)
-- Author : Rakesh Pernati
-- Purpose:
--   - Removes "agreementDeclineImageUrl" from tenant_attr
--   - Removes "agreementsColors" from tenant_attr->ux
--   - Handles both HAP tenant and OTHER tenants
-- JIRA Ticket : BEN-6
-- =====================================================================

DO $$
DECLARE
    hap_tenant_code TEXT := '<HAP-TENANT-CODE>';    -- input 1
    other_tenant_codes TEXT[] := ARRAY['<KP-TENANT-CODE>', '<WATCO-TENANT-CODE>','NAVITUS-TENANT-CODE'];      -- input 2
	row_count INT;  -- ✅ declare row_count
BEGIN
    RAISE NOTICE '================ ROLLBACK STARTED ================';

    -- =====================================================================
    -- Rollback HAP tenant
    -- =====================================================================
    UPDATE tenant.tenant t
    SET tenant_attr = (t.tenant_attr - 'agreementDeclineImageUrl')::jsonb
    WHERE t.tenant_code = hap_tenant_code
      AND t.delete_nbr = 0
      AND t.tenant_attr ? 'agreementDeclineImageUrl';

    GET DIAGNOSTICS row_count = ROW_COUNT;
    RAISE NOTICE 'HAP Tenant: Removed agreementDeclineImageUrl → % rows affected', row_count;

    UPDATE tenant.tenant t
    SET tenant_attr = jsonb_set(
                        t.tenant_attr,
                        '{ux}',
                        (t.tenant_attr->'ux') - 'agreementsColors'
                      )
    WHERE t.tenant_code = hap_tenant_code
      AND t.delete_nbr = 0
      AND (t.tenant_attr->'ux') ? 'agreementsColors';

    GET DIAGNOSTICS row_count = ROW_COUNT;
    RAISE NOTICE 'HAP Tenant: Removed agreementsColors → % rows affected', row_count;

    -- =====================================================================
    -- Rollback OTHER tenants
    -- =====================================================================
    UPDATE tenant.tenant t
    SET tenant_attr = (t.tenant_attr - 'agreementDeclineImageUrl')::jsonb
    WHERE t.tenant_code = ANY(other_tenant_codes)
      AND t.delete_nbr = 0
      AND t.tenant_attr ? 'agreementDeclineImageUrl';

    GET DIAGNOSTICS row_count = ROW_COUNT;
    RAISE NOTICE 'Other Tenants: Removed agreementDeclineImageUrl → % rows affected', row_count;

    UPDATE tenant.tenant t
    SET tenant_attr = jsonb_set(
                        t.tenant_attr,
                        '{ux}',
                        (t.tenant_attr->'ux') - 'agreementsColors'
                      )
    WHERE t.tenant_code = ANY(other_tenant_codes)
      AND t.delete_nbr = 0
      AND (t.tenant_attr->'ux') ? 'agreementsColors';

    GET DIAGNOSTICS row_count = ROW_COUNT;
    RAISE NOTICE 'Other Tenants: Removed agreementsColors → % rows affected', row_count;

    RAISE NOTICE '================ ROLLBACK COMPLETED ================';
END $$;
