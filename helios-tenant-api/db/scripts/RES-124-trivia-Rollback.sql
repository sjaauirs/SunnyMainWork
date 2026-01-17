DO
$$
DECLARE
    v_tenant_codes          TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant_code           TEXT;

    v_env                   TEXT := 'QA'; -- DEV | QA | UAT | INTEG | PROD

    v_trivia_mobile_image   TEXT := 'common_trivia.png';
    v_trivia_desktop_image  TEXT := 'common_trivia.png';

    v_env_specific_url      TEXT;
BEGIN
    -- Resolve environment-specific base URL
    CASE UPPER(v_env)
        WHEN 'DEV'   THEN v_env_specific_url := 'https://app-static.dev.sunnyrewards.com/images/';
        WHEN 'QA'    THEN v_env_specific_url := 'https://app-static.qa.sunnyrewards.com/images/';
        WHEN 'UAT'   THEN v_env_specific_url := 'https://app-static.uat.sunnyrewards.com/images/';
        WHEN 'INTEG' THEN v_env_specific_url := 'https://app-static.integ.sunnyrewards.com/images/';
        WHEN 'PROD'  THEN v_env_specific_url := 'https://app-static.sunnyrewards.com/images/';
        ELSE
            RAISE EXCEPTION
                'Invalid environment [%]. Choose from DEV, QA, UAT, INTEG, PROD.', v_env;
    END CASE;

    -- Rollback update
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant
        SET tenant_att =
            jsonb_set(
                jsonb_set(
                    tenant_att,
                    '{TriviaMobileImage}',
                    to_jsonb(v_env_specific_url || v_trivia_mobile_image),
                    true
                ),
                '{TriviaDesktopImage}',
                to_jsonb(v_env_specific_url || v_trivia_desktop_image),
                true
            )
        WHERE tenant_code = v_tenant_code;

        RAISE NOTICE
            'Rollback applied: Trivia images reset to common_trivia.png for tenant %, env %',
            v_tenant_code, v_env;
    END LOOP;
END;
$$;
