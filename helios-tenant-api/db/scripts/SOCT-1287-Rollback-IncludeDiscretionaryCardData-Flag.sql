DO $$
BEGIN
    RAISE NOTICE '🔄 Starting rollback: Removing IncludeDiscretionaryCardData from all tenants...';

    UPDATE tenant.tenant
    SET tenant_option_json = tenant_option_json #- '{benefitsOptions,IncludeDiscretionaryCardData}'
    WHERE tenant_option_json #> '{benefitsOptions,IncludeDiscretionaryCardData}' IS NOT NULL;

    RAISE NOTICE '✅ Successfully removed IncludeDiscretionaryCardData from all tenants.';
END $$;
