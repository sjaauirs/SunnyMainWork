-- Update CalendarSelectedDayColor, inProgressTextFgColor, Trivia Images, isRedirectSignout, declineRedirectLink
DO $$
DECLARE
  v_tenant_code text := '<WATCO-TENANT-CODE>';
  v_env TEXT := '<ENV>';  -- DEV, QA, UAT, INTEG, PROD
  v_env_specific_url TEXT;
BEGIN
  -- Resolve environment-specific static URL
  CASE v_env
      WHEN 'DEV'   THEN v_env_specific_url := 'https://app-static.dev.sunnyrewards.com';
      WHEN 'QA'    THEN v_env_specific_url := 'https://app-static.qa.sunnyrewards.com';
      WHEN 'UAT'   THEN v_env_specific_url := 'https://app-static.uat.sunnyrewards.com';
      WHEN 'INTEG' THEN v_env_specific_url := 'https://app-static.integ.sunnyrewards.com';
      WHEN 'PROD'  THEN v_env_specific_url := 'https://app-static.sunnyrewards.com';
      ELSE
          RAISE EXCEPTION 'Invalid environment [%]. Choose from DEV, QA, UAT, INTEG, PROD.', v_env;
  END CASE;

  UPDATE tenant.tenant
  SET tenant_attr =
      jsonb_set(
        jsonb_set(
          jsonb_set(
            jsonb_set(
              jsonb_set(
                jsonb_set(
                  tenant_attr,
                  '{ux,themeColors,CalendarSelectedDayColor}', '"#1B416D"', true
                ),
                '{ux,taskTileColors,inProgressTextFgColor}', '"#1B416D"', true
              ),
              '{TriviaDesktopImage}', to_jsonb(v_env_specific_url || '/public/images/watco_desktop_trivia.png'), true
            ),
            '{TriviaMobileImage}', to_jsonb(v_env_specific_url || '/public/images/watco_mobile_trivia.png'), true
          ),
          '{isRedirectSignout}', to_jsonb(true), true
        ),
        '{declineRedirectLink}', '"https://cqa3.benefitfocus.com/member/control/homePageAction?method=execute"', true
      )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'Updated Watco tenant [%] with env [%] URLs and redirect settings', v_tenant_code, v_env;
END $$;


DO $$
DECLARE
  v_tenant_code text := '<KP-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr,
      '{ux,themeColors,CalendarSelectedDayColor}', '"#003B71"', true
  )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;

DO $$
DECLARE
  v_tenant_code text := '<NAVITUS-TENANT-CODE>';
  v_env TEXT := '<ENV>';  -- DEV, QA, UAT, INTEG, PROD
  v_env_specific_url TEXT;
BEGIN
  -- Resolve environment-specific static URL
  CASE v_env
      WHEN 'DEV'   THEN v_env_specific_url := 'https://app-static.dev.sunnyrewards.com';
      WHEN 'QA'    THEN v_env_specific_url := 'https://app-static.qa.sunnyrewards.com';
      WHEN 'UAT'   THEN v_env_specific_url := 'https://app-static.uat.sunnyrewards.com';
      WHEN 'INTEG' THEN v_env_specific_url := 'https://app-static.integ.sunnyrewards.com';
      WHEN 'PROD'  THEN v_env_specific_url := 'https://app-static.sunnyrewards.com';
      ELSE
          RAISE EXCEPTION 'Invalid environment [%]. Choose from DEV, QA, UAT, INTEG, PROD.', v_env;
  END CASE;

  UPDATE tenant.tenant
  SET tenant_attr =
      jsonb_set(
        jsonb_set(
          jsonb_set(
            tenant_attr,
            '{ux,themeColors,CalendarSelectedDayColor}', '"#1B416D"', true
          ),
          '{TriviaDesktopImage}', to_jsonb(v_env_specific_url || '/public/images/navitus_desktop_trivia.png'), true
        ),
        '{TriviaMobileImage}', to_jsonb(v_env_specific_url || '/public/images/navitus_mobile_trivia.png'), true
      )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'Updated Navitus tenant [%] with env [%] URLs', v_tenant_code, v_env;
END $$;

DO $$
DECLARE
  v_tenant_code text := '<SUNNY-TENANT-CODE>';
  v_env TEXT := '<ENV>';  -- DEV, QA, UAT, INTEG, PROD
  v_env_specific_url TEXT;
BEGIN
  -- Resolve environment-specific static URL
  CASE v_env
      WHEN 'DEV'   THEN v_env_specific_url := 'https://app-static.dev.sunnyrewards.com';
      WHEN 'QA'    THEN v_env_specific_url := 'https://app-static.qa.sunnyrewards.com';
      WHEN 'UAT'   THEN v_env_specific_url := 'https://app-static.uat.sunnyrewards.com';
      WHEN 'INTEG' THEN v_env_specific_url := 'https://app-static.integ.sunnyrewards.com';
      WHEN 'PROD'  THEN v_env_specific_url := 'https://app-static.sunnyrewards.com';
      ELSE
          RAISE EXCEPTION 'Invalid environment [%]. Choose from DEV, QA, UAT, INTEG, PROD.', v_env;
  END CASE;

  UPDATE tenant.tenant
  SET tenant_attr =
      jsonb_set(
        jsonb_set(
          jsonb_set(
            tenant_attr,
            '{ux,themeColors,CalendarSelectedDayColor}', '"#1B416D"', true
          ),
          '{TriviaDesktopImage}', to_jsonb(v_env_specific_url || '/public/images/sunny_desktop_trivia.png'), true
        ),
        '{TriviaMobileImage}', to_jsonb(v_env_specific_url || '/public/images/sunny_mobile_trivia.png'), true
      )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'Updated Sunny tenant [%] with env [%] URLs', v_tenant_code, v_env;
END $$;
