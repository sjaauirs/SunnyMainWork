-- ============================================================================
-- 🚀 Script    : Fix tenant_attr properties for Watco
-- 📌 Purpose   : Fix tenant_attr properties for Watco
-- 👨‍💻 Author    : Neel
-- 📅 Date      : 2025-12-05
-- 🧾 Jira      : SUN-1223
-- ⚠️ Inputs    : v_tenant_codes[]
-- 📤 Output    : Updates tenant_attr JSON structure
-- 📝 Notes     :
--    - Adds or overwrites enableStoresSection in tenant_attr.
--    - Idempotent: safe to run multiple times.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'WATCO-TENANT-CODE'
    ];
     v_tenant TEXT; 
	 v_env TEXT :='ENV';
	v_env_specific_url TEXT;
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

    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP
        
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
                jsonb_set(
                  jsonb_set(
                    jsonb_set(
                      jsonb_set(
                        jsonb_set(
                          jsonb_set(
                            jsonb_set(
                              tenant_attr,
                              -- 1. ux.triviaColors.nextButtonBgColor = "#FFC907"
                              '{ux,triviaColors,nextButtonBgColor}',
                              to_jsonb('#FFC907'::text),
                              true
                            ),
                            -- 2. ux.triviaColors.nextButtonBorderColor = "#FFC907"
                            '{ux,triviaColors,nextButtonBorderColor}',
                            to_jsonb('#FFC907'::text),
                            true
                          ),
                          -- 3. ux.triviaColors.nextButtonTextColor = "#000000"
                          '{ux,triviaColors,nextButtonTextColor}',
                          to_jsonb('#000000'::text),
                          true
                        ),
                        -- 4. ux.button.primaryTextColor = "#000000"
                        '{ux,button,primaryTextColor}',
                        to_jsonb('#000000'::text),
                        true
                      ),
                      -- 5. ux.taskTileColors.checkBoxBgColor = "#1B416D"
                      '{ux,taskTileColors,checkBoxBgColor}',
                      to_jsonb('#1B416D'::text),
                      true
                    ),
                    -- 6. ux.button.primaryBgColor = "#FFC907"
                    '{ux,button,primaryBgColor}',
                    to_jsonb('#FFC907'::text),
                    true
                  ),
                  -- 7. ux.themeColors.accent1 = "#FFC907"
                  '{ux,themeColors,accent1}',
                  to_jsonb('#FFC907'::text),
                  true
                ),
                -- 8. ux.themeColors.accent3 = "#000000"
                '{ux,themeColors,accent3}',
                to_jsonb('#000000'::text),
                true
              ),
            update_ts = NOW()
        WHERE tenant_code = v_tenant
          AND delete_nbr = 0;

        RAISE NOTICE '✅ Updated attributes for Watco tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🏁 Completed updating tenant attributes for all WATCO tenants.';
END $$;