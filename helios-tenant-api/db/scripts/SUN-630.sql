DO $$
DECLARE
    v_target_tenants TEXT[] := ARRAY['<KP-TENANT-CODE-1>', '<KP-TENANT-CODE-2>'];  -- Replace with actual tenant codes
    v_env TEXT := '<ENV>';  -- DEV, QA, UAT, INTEG, or PROD
    v_env_specific_url TEXT;
    v_json JSONB;
    v_tenant_code TEXT;

    v_mobile_image TEXT;
    v_desktop_image TEXT;
BEGIN
    -- Resolve environment-specific static URL
    CASE v_env
        WHEN 'DEV' THEN
            v_env_specific_url := 'https://app-static.dev.sunnyrewards.com';
        WHEN 'QA' THEN
            v_env_specific_url := 'https://app-static.qa.sunnyrewards.com';
        WHEN 'UAT' THEN
            v_env_specific_url := 'https://app-static.uat.sunnyrewards.com';
        WHEN 'INTEG' THEN
            v_env_specific_url := 'https://app-static.integ.sunnyrewards.com';
        WHEN 'PROD' THEN
            v_env_specific_url := 'https://app-static.sunnyrewards.com';
        ELSE
            RAISE EXCEPTION 'Invalid environment [%]. Choose from DEV, QA, UAT, INTEG, PROD.', v_env;
    END CASE;

    -- Loop through all active tenants
    FOR v_tenant_code IN
        SELECT tenant_code FROM tenant.tenant WHERE delete_nbr = 0
    LOOP
        -- Get existing tenant_attr
        SELECT tenant_attr INTO v_json
        FROM tenant.tenant
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        IF v_tenant_code = ANY(v_target_tenants) THEN
            v_mobile_image := v_env_specific_url || '/public/images/kp_mobile_trivia.png';
            v_desktop_image := v_env_specific_url || '/public/images/kp_desktop_trivia.png';
        ELSE
            v_mobile_image := v_env_specific_url || '/public/images/common_trivia.png';
            v_desktop_image := v_env_specific_url || '/public/images/common_trivia.png';
        END IF;

        -- Inject new fields into JSONB
        v_json := jsonb_set(
                      jsonb_set(
                          v_json,
                          '{TriviaMobileImage}',
                          to_jsonb(v_mobile_image),
                          true
                      ),
                      '{TriviaDesktopImage}',
                          to_jsonb(v_desktop_image),
                          true
                  );

        -- Save updated JSONB
        UPDATE tenant.tenant
        SET tenant_attr = v_json
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE 'Updated Trivia Images for tenant: %', v_tenant_code;
    END LOOP;
END $$;