DO $$
DECLARE
    v_tenant_code TEXT := '<TENANT_CODE>'; -- 🔁 Replace with your tenant code
    v_exists      BOOLEAN;
BEGIN
    -- 🔍 Check if tenant exists
    SELECT EXISTS (
        SELECT 1 FROM tenant.tenant
        WHERE tenant_code = v_tenant_code
    ) INTO v_exists;

    IF v_exists THEN
        RAISE NOTICE '✔️ Tenant "%" found. Updating IncludeDiscretionaryCardData to true...', v_tenant_code;

        UPDATE tenant.tenant
        SET tenant_option_json = jsonb_set(
            tenant_option_json,
            '{benefitsOptions,IncludeDiscretionaryCardData}',
            'true'::jsonb,
            true
        )
        WHERE tenant_code = v_tenant_code;

        RAISE NOTICE '✅ Successfully updated IncludeDiscretionaryCardData = true for tenant: %', v_tenant_code;
    ELSE
        RAISE EXCEPTION '❌ Tenant "%" not found in tenant.tenant table.', v_tenant_code;
    END IF;
END $$;
