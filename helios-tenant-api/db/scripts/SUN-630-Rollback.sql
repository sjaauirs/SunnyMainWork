DO $$
DECLARE
    v_json JSONB;
    v_tenant_code TEXT;
BEGIN
    -- Loop through all active tenants
    FOR v_tenant_code IN
        SELECT tenant_code FROM tenant.tenant WHERE delete_nbr = 0
    LOOP
        -- Get existing tenant_attr
        SELECT tenant_attr INTO v_json
        FROM tenant.tenant
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        -- Remove both attributes from JSONB
        v_json := v_json - 'TriviaMobileImage';
        v_json := v_json - 'TriviaDesktopImage';

        -- Update the tenant_attr
        UPDATE tenant.tenant
        SET tenant_attr = v_json
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE 'Removed Trivia attributes from tenant: %', v_tenant_code;
    END LOOP;
END $$;
