DO $$
DECLARE
  v_tenant_code TEXT := '<SUNNY-TENANT-CODE>';  -- <-- CHANGE ME
BEGIN
  ----------------------------------------------------------------
  -- R1) Remove ux.footerColors & ux.headerColors
  ----------------------------------------------------------------
  UPDATE tenant.tenant
  SET tenant_attr = (tenant_attr::jsonb - '{ux,footerColors}') - '{ux,headerColors}'
  WHERE tenant_code = v_tenant_code AND delete_nbr = 0;

  ----------------------------------------------------------------
  -- R2) Remove ONLY ux.onboardingColors.stepTextColor
  ----------------------------------------------------------------
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr::jsonb,
      '{ux,onboardingColors}',
      COALESCE(tenant_attr::jsonb #> '{ux,onboardingColors}', '{}'::jsonb) - 'stepTextColor',
      true
  )
  WHERE tenant_code = v_tenant_code AND delete_nbr = 0;

  ----------------------------------------------------------------
  -- R3) Remove cardActivationSuccessImage
  ----------------------------------------------------------------
  UPDATE tenant.tenant
  SET tenant_attr = tenant_attr::jsonb - 'cardActivationSuccessImage'
  WHERE tenant_code = v_tenant_code AND delete_nbr = 0;

  ----------------------------------------------------------------
  -- R4) Remove headerImageUrls
  ----------------------------------------------------------------
  UPDATE tenant.tenant
  SET tenant_attr = tenant_attr::jsonb - 'headerImageUrls'
  WHERE tenant_code = v_tenant_code AND delete_nbr = 0;

  ----------------------------------------------------------------
  -- R5) Remove top-level booleans
  ----------------------------------------------------------------
  UPDATE tenant.tenant
  SET tenant_attr = (tenant_attr::jsonb - 'showCardCopyRightText') - 'includeLanguageDropdown'
  WHERE tenant_code = v_tenant_code AND delete_nbr = 0;

  ----------------------------------------------------------------
  -- R6) Rollback added commonColors keys 
  ----------------------------------------------------------------
 UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr::jsonb,
      '{ux,commonColors}',
      (
        COALESCE(tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
        - 'textColor'
        - 'textColor2'
        - 'textColor3'
        - 'textColor4'
        - 'textColor5'
        - 'textColor6'
        - 'textColor7'
        - 'borderColor'
        - 'borderColor1'
        - 'borderColor2'
        - 'borderColor3'
        - 'borderColor4'
        - 'borderColor5'
        - 'primaryColor'
        - 'screenBgColor'
        - 'contentBgColor'
        - 'secondaryColor'
        - 'buttonTextColor'
        - 'contentBgColor2'
        - 'contentBgColor3'
        - 'buttonTextColor2'
        - 'errorBorderColor'
        - 'iconInActiveColor'
        - 'disableButtonBgColor'
        - 'disableButtonBgColor1'
      ),
      true
  )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

  RAISE NOTICE 'Rollback applied for tenant=%.', v_tenant_code;
END $$;
