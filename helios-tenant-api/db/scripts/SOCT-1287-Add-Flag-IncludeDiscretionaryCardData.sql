DO $$
BEGIN
    RAISE NOTICE '🔧 Updating all tenants: Adding IncludeDiscretionaryCardData = false';

    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json,
        '{benefitsOptions,IncludeDiscretionaryCardData}',
        'false'::jsonb,
        true
    );

    RAISE NOTICE '✅ Successfully updated all tenants with IncludeDiscretionaryCardData = false';
END $$;
