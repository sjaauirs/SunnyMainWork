
-- ============================================================================
-- 🚀 Script: update tenant_attr for trivia url
-- 📌 Purpose: updating trivia image url
-- 🧑 Author  : Kawalpreet Kaur
-- 📅 Date    : 2025-12-17
-- 🧾 Jira    : RES-124
-- ⚠️  Inputs: Tenant_code for navitus and image upload in S3
-- ============================================================================
DO
$$
DECLARE
    v_tenant_codes          TEXT[] := ARRAY[
        '<NAVITUS_TENANT_CODE>'
    ];
    v_tenant_code           TEXT;

    v_env                   TEXT := 'DEV'; -- DEV | QA | UAT | INTEG | PROD

    v_trivia_mobile_image   TEXT := 'Navitus_trivia.png';
    v_trivia_desktop_image  TEXT := 'Navitus_trivia.png';

    v_env_specific_url      TEXT;
BEGIN
    -- Resolve environment-specific base URL
    CASE UPPER(v_env)
        WHEN 'DEV'   THEN v_env_specific_url := 'https://app-static.dev.sunnyrewards.com/public/images/';
        WHEN 'QA'    THEN v_env_specific_url := 'https://app-static.qa.sunnyrewards.com/public/images/';
        WHEN 'UAT'   THEN v_env_specific_url := 'https://app-static.uat.sunnyrewards.com/public/images/';
        WHEN 'INTEG' THEN v_env_specific_url := 'https://app-static.integ.sunnyrewards.com/public/images/';
        WHEN 'PROD'  THEN v_env_specific_url := 'https://app-static.sunnyrewards.com/public/images/';
        ELSE
            RAISE EXCEPTION
                'Invalid environment [%]. Choose from DEV, QA, UAT, INTEG, PROD.', v_env;
    END CASE;

    -- Loop through tenant codes
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant
        SET tenant_attr =
            jsonb_set(
                jsonb_set(
                    tenant_attr,
                    '{TriviaMobileImage}',
                    to_jsonb(v_env_specific_url  || v_trivia_mobile_image),
                    true
                ),
                '{TriviaDesktopImage}',
                to_jsonb(v_env_specific_url || v_trivia_desktop_image),
                true
            )
        WHERE tenant_code = v_tenant_code and delete_nbr=0;

        RAISE NOTICE
            'Updated Trivia images for tenant %, env %',
            v_tenant_code, v_env;
    END LOOP;
END;
$$;
