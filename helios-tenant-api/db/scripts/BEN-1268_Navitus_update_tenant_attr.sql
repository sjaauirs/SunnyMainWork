-- =====================================================================================================
-- 🚀 Script       : Add/Update agreementDeclineImageUrl in tenant_attr JSONB
-- 📌 Purpose      : Ensures that tenant_attr JSONB for the given tenant contains the following key:
--                   - agreementDeclineImageUrl (added or updated with correct URL)
-- 🧑 Author       : Rakesh Pernati
-- 🗓️ Date         : 2025-12-02
-- 🎫 JIRA Ticket  : BEN-1268
-- ⚙️ Inputs       : Replace <NAVITUS-TENANT-CODE> with the actual tenant code
-- 📤 Output       : tenant_attr JSONB with updated agreementDeclineImageUrl
-- 📝 Notes        :
--   - Safe to run multiple times (idempotent)
--   - If no tenant found or tenant_attr is empty, displays a warning
-- =====================================================================================================

DO $$
DECLARE
    v_env TEXT := '<ENVIRONMENT>'; -- 👈 Set environment here
    v_tenant_codes TEXT[] := ARRAY[
        '<NAVITUS-TENANT-CODE>',
        '<NAVITUS-TENANT-CODE>'
    ];

    v_tenant_code TEXT;
    v_env_specific_url TEXT;
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_updated BOOLEAN;

    v_env_specific_path TEXT := '/cms/images/';
    v_file_extension TEXT := '.png';
    v_filename TEXT := 'navitus_agreement_decline_image';

    v_agreementDeclineImageUrl TEXT;
BEGIN

    -- Resolve environment-specific base URL
    CASE v_env
        WHEN 'DEV'   THEN v_env_specific_url := 'https://app-static.dev.sunnyrewards.com';
        WHEN 'QA'    THEN v_env_specific_url := 'https://app-static.qa.sunnyrewards.com';
        WHEN 'UAT'   THEN v_env_specific_url := 'https://app-static.uat.sunnyrewards.com';
        WHEN 'INTEG' THEN v_env_specific_url := 'https://app-static.integ.sunnyrewards.com';
        WHEN 'PROD'  THEN v_env_specific_url := 'https://app-static.sunnyrewards.com';
        ELSE
            RAISE EXCEPTION 'Invalid environment [%]. Use DEV/QA/UAT/INTEG/PROD.', v_env;
    END CASE;


    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        v_updated := FALSE;

        -- Fetch existing tenant_attr
        SELECT tenant_attr
        INTO v_old_attr
        FROM tenant.tenant
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
          AND tenant_attr IS NOT NULL
          AND tenant_attr::text <> '{}';

        IF NOT FOUND THEN
            RAISE WARNING '⚠ No tenant found or tenant_attr empty for tenant: %', v_tenant_code;
            CONTINUE;
        END IF;

        v_new_attr := v_old_attr;

        -- Build new URL
        v_agreementDeclineImageUrl :=
            v_env_specific_url || v_env_specific_path || v_tenant_code || '/' || v_filename || v_file_extension;

        -- Add or Update agreementDeclineImageUrl
        v_new_attr := jsonb_set(
            v_new_attr,
            '{agreementDeclineImageUrl}',
            to_jsonb(v_agreementDeclineImageUrl),
            TRUE
        );

        v_updated := TRUE;

        RAISE NOTICE '🔄 agreementDeclineImageUrl added/updated for tenant %', v_tenant_code;


        -- Update database if modified
        IF v_updated THEN
            UPDATE tenant.tenant
            SET tenant_attr = v_new_attr
            WHERE tenant_code = v_tenant_code
              AND delete_nbr = 0;

            RAISE NOTICE '✅ tenant_attr updated successfully for tenant %', v_tenant_code;
        END IF;

    END LOOP;

EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION '❌ Failed to update tenant_attr: %', SQLERRM;

END $$;
