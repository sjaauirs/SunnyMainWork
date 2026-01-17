--===============================================================================
-- Script  : Update tenant_attr JSON configuration
-- Jira    : BEN-19
-- Purpose : Apply UI/UX and feature configuration updates for tenant 'Watco'
--           with environment-specific image URLs (DEV / QA / UAT / INTEG / PROD).
--
-- Updates Included:
--   1. Add ux.footerColors (footerBgColor, footerTextColor)
--   2. Add ux.headerColors (headerBgColor, headerTextColor, headerTopBorderColor,
--      headerBottomBorderColor)
--   3. Add ux.onboardingColors.stepTextColor
--   4. Add cardActivationSuccessImage (env-specific URL)
--   5. Add headerImageUrls (headerMobileIconUrl, headerDesktopIconUrl, env-specific URLs)
--   6. Add showCardCopyRightText (true)
--   7. Add includeLanguageDropdown (false)
--   8. Add benefitsCardArtUrl (env-specific URL)
--
--===============================================================================
-- ⚠️ Caution: Before executing
--   - Validate target environment (DEV / QA / UAT / INTEG / PROD)
--   - Ensure tenant_code is correct (Watco tenant)
--   - Rollback scripts are available for each update
--   - Use transaction control (BEGIN/COMMIT) if applying multiple updates together
--===============================================================================


DO $$
DECLARE
  -- === INPUTS ===
  v_tenant_code TEXT := '<WATCO-TENANT-CODE>';  -- <-- CHANGE ME
  v_env         TEXT := '<ENVIROMENT>';                                    -- <-- one of DEV/QA/UAT/INTEG/PROD

  -- === INTERNALS ===
  v_env_host TEXT;             -- e.g., https://app-static.dev.sunnyrewards.com
  v_img_base TEXT;             -- e.g., https://app-static.dev.sunnyrewards.com/public/images
  v_icon_url  TEXT;            -- hap_icon.png
  v_check_url TEXT;            -- check-circle.png
BEGIN
  -- Resolve environment host
  CASE v_env
    WHEN 'DEV'   THEN v_env_host := 'https://app-static.dev.sunnyrewards.com';
    WHEN 'QA'    THEN v_env_host := 'https://app-static.qa.sunnyrewards.com';
    WHEN 'UAT'   THEN v_env_host := 'https://app-static.uat.sunnybenefits.com';
    WHEN 'INTEG' THEN v_env_host := 'https://app-static.integ.sunnyrewards.com';
    WHEN 'PROD'  THEN v_env_host := 'https://app-static.sunnyrewards.com';
    ELSE
      RAISE EXCEPTION 'Invalid environment [%]. Choose DEV, QA, UAT, INTEG, or PROD.', v_env;
  END CASE;

  v_img_base := v_env_host || '/public/images';
  v_icon_url  := v_img_base || '/hap_icon.png';
  v_check_url := v_img_base || '/check-circle.png';

 ----------------------------------------------------------------
  -- 1) ux.footerColors + ux.headerColors
  ----------------------------------------------------------------
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      jsonb_set(
        tenant_attr::jsonb,
        '{ux,footerColors}',
        '{
          "footerBgColor": "#0D1C3D",
          "footerTextColor": "#FFFFFF"
        }'::jsonb,
        true
      ),
      '{ux,headerColors}',
      '{
        "headerBgColor": "#0D1C3D",
        "headerTextColor": "#FFFFFF",
        "headerTopBorderColor": "#0D1C3D",
        "headerBottomBorderColor": "#0D1C3D"
      }'::jsonb,
      true
  )
  WHERE tenant_code = v_tenant_code AND delete_nbr = 0;

  ----------------------------------------------------------------
  -- 2) Add ux.onboardingColors.stepTextColor and stepActiveColor (merge into object)
  ----------------------------------------------------------------
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr::jsonb,
      '{ux,onboardingColors}',
      COALESCE(tenant_attr::jsonb #> '{ux,onboardingColors}', '{}'::jsonb)
      || jsonb_build_object('stepTextColor', '#677083'),
      true
  )
  WHERE tenant_code = v_tenant_code AND delete_nbr = 0;
  
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr::jsonb,
      '{ux,onboardingColors}',
      COALESCE(tenant_attr::jsonb #> '{ux,onboardingColors}', '{}'::jsonb)
      || jsonb_build_object('stepActiveColor', '#0078B3'),
      true
  )
  WHERE tenant_code = v_tenant_code AND delete_nbr = 0;

  ----------------------------------------------------------------
  -- 3) Add cardActivationSuccessImage (env-aware)
  ----------------------------------------------------------------
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr::jsonb,
      '{cardActivationSuccessImage}',
      to_jsonb(v_check_url),
      true
  )
  WHERE tenant_code = v_tenant_code AND delete_nbr = 0;

  ----------------------------------------------------------------
  -- 4) Add headerImageUrls (env-aware)
  ----------------------------------------------------------------
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr::jsonb,
      '{headerImageUrls}',
      jsonb_build_object(
		  'headerMobileIconUrl',  ''::text,
		  'headerDesktopIconUrl', ''::text
	  ),
      true
  )
  WHERE tenant_code = v_tenant_code AND delete_nbr = 0;

  ----------------------------------------------------------------
  -- 5) Add top-level booleans: showCardCopyRightText & includeLanguageDropdown
  ----------------------------------------------------------------
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      jsonb_set(
        tenant_attr::jsonb,
        '{showCardCopyRightText}', 'false'::jsonb, true
      ),
      '{includeLanguageDropdown}', 'false'::jsonb, true
  )
  WHERE tenant_code = v_tenant_code AND delete_nbr = 0;
  
  ----------------------------------------------------------------
  -- 6) Expand ux.commonColors (merge/overwrite with new keys)
  ----------------------------------------------------------------
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr::jsonb,
      '{ux,commonColors}',
      COALESCE(tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
      || '{
            "textColor": "#0D1C3D",
            "textColor2": "#677083",
            "textColor3": "#0078b3",
            "textColor4": "#677083",
            "textColor5": "#0D1C3D",
            "textColor6": "#0078b3",
            "textColor7": "#677083",
            "borderColor": "#0078b3",
            "buttonColor": "#0078b3",
            "borderColor1": "#D3D6DC",
            "borderColor2": "#0078b3",
            "borderColor3": "#E9EBEE",
            "borderColor4": "#E9EBEE",
            "borderColor5": "#D3D6DC",
            "button1Color": "#FFC907",
            "primaryColor": "#003B71",
            "screenBgColor": "#F7F7F7",
            "contentBgColor": "#FFFFFF",
            "secondaryColor": "#66615C",
            "buttonTextColor": "#FFFFFF",
            "contentBgColor2": "#E9EBEE",
            "contentBgColor3": "#E9EBEE",
            "contentBgColor4": "#E9EBEE",
            "button1TextColor": "#000000",
            "buttonTextColor2": "#0D1C3D",
            "errorBorderColor": "#BE2D00",
            "iconInActiveColor": "#868C92",
            "disableButtonBgColor": "#E9EBEE",
            "disableButtonBgColor1": "#868c92",
            "paginationDotActiveColor": "#0D1C3D",
            "paginationDotNonActiveColor": "#D3D6DC"
      }'::jsonb,
      true
  )
  WHERE tenant_code = v_tenant_code AND delete_nbr = 0;

  RAISE NOTICE 'Updates applied for tenant=% and env=% (base=%).', v_tenant_code, v_env, v_img_base;
END $$;
