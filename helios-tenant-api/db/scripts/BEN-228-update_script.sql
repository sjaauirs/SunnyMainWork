-- ===================================================================================
-- Author      : Pernati Rakesh
-- Purpose     : Update tenant configuration for specific tenant_code 
--               - Enable DST and set UTC offset
--               - Remove 'myRewards' from benefitsOptions.hamburgerMenu
--               - Add 'noAvailblePurseImageUrl' key in tenant_attr (env-specific URL)
--               - Update period_start_ts and period_end_ts
-- Jira Task   : BEN-228
-- ===================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';  -- 👈 replace with your tenant code
    v_env TEXT := '<YOUR_ENVIRONMENT>';         -- Replace with: DEV / QA / UAT / INTEG / PROD
    v_env_specific_url TEXT;
    v_purse_image_url TEXT;
BEGIN
    -- Determine environment-specific base URL
    CASE v_env
        WHEN 'DEV'   THEN v_env_specific_url := 'https://app-static.dev.sunnyrewards.com';
        WHEN 'QA'    THEN v_env_specific_url := 'https://app-static.qa.sunnyrewards.com';
        WHEN 'UAT'   THEN v_env_specific_url := 'https://app-static.uat.sunnyrewards.com';
        WHEN 'INTEG' THEN v_env_specific_url := 'https://app-static.integ.sunnyrewards.com';
        WHEN 'PROD'  THEN v_env_specific_url := 'https://app-static.sunnyrewards.com';
        ELSE
            RAISE EXCEPTION 'Invalid environment [%]. Please choose from DEV, QA, UAT, INTEG, PROD.', v_env;
    END CASE;

    -- Construct env-specific URL
    v_purse_image_url := v_env_specific_url || '/assets/icons/no_available_purse.png';

    RAISE NOTICE 'Starting update for tenant_code=%, env=%', v_tenant_code, v_env;

    -- Safety check
    IF NOT EXISTS (
        SELECT 1
        FROM tenant.tenant
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
    ) THEN
        RAISE WARNING 'No active tenant found with tenant_code=% (delete_nbr=0)', v_tenant_code;
        RETURN;
    END IF;

    -- Perform update
    UPDATE tenant.tenant
    SET 
        dst_enabled = TRUE,
        utc_time_offset = 'UTC-06:00',
        period_start_ts = TIMESTAMP '2026-01-01 00:00:00',
        period_end_ts   = TIMESTAMP '2026-12-31 23:59:59',
        -- 🔹 Remove "myRewards" from benefitsOptions.hamburgerMenu
        tenant_option_json = jsonb_set(
            tenant_option_json,
            '{benefitsOptions,hamburgerMenu}',
            (
                SELECT jsonb_agg(elem)
                FROM jsonb_array_elements(
                    tenant_option_json->'benefitsOptions'->'hamburgerMenu'
                ) elem
                WHERE elem::text <> '"myRewards"'
            ),
            FALSE
        ),
        -- 🔹 Add env-specific noAvailblePurseImageUrl to tenant_attr
        tenant_attr = jsonb_set(
            tenant_attr,
            '{noAvailblePurseImageUrl}',
            to_jsonb(v_purse_image_url::text),
            TRUE
        )
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE 'Update completed successfully for tenant_code=%, env=%', v_tenant_code, v_env;
END $$;
