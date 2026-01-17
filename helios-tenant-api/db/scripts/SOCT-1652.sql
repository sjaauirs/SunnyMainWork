-- Lower environments
DO $$
DECLARE
    v_tenant_code text := '<KP-TENANT-CODE>';
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr =
      jsonb_set(                           -- merge for "en-US"
        jsonb_set(                         -- merge for "es"
          tenant_attr,
          '{languageSpecificContent,es}',
          COALESCE(tenant_attr->'languageSpecificContent'->'es', '{}'::jsonb)
          || jsonb_build_object(
               'kpRedirectLink',  'https://espanol-hreg1.kaiserpermanente.org/es/colorado/secure/inner-door',
               'kpGaRedirectLink','https://espanol-hreg2.kaiserpermanente.org/es/georgia/secure/inner-door'
             ),
          true
        ),
        '{languageSpecificContent,en-US}',
        COALESCE(tenant_attr->'languageSpecificContent'->'en-US', '{}'::jsonb)
        || jsonb_build_object(
             'kpRedirectLink',  'https://hreg1.kaiserpermanente.org/colorado/secure/inner-door',
             'kpGaRedirectLink','https://hreg2.kaiserpermanente.org/georgia/secure/inner-door'
           ),
        true
      )
    WHERE tenant_attr ? 'languageSpecificContent'
      AND tenant_code = v_tenant_code;
END$$;

-- Production only
DO $$
DECLARE
    v_tenant_code text := '<KP-TENANT-CODE>';
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr =
      jsonb_set(                           -- merge for "en-US"
        jsonb_set(                         -- merge for "es"
          tenant_attr,
          '{languageSpecificContent,es}',
          COALESCE(tenant_attr->'languageSpecificContent'->'es', '{}'::jsonb)
          || jsonb_build_object(
               'kpRedirectLink',  'https://espanol.kaiserpermanente.org/es/colorado/secure/inner-door',
               'kpGaRedirectLink',''
             ),
          true
        ),
        '{languageSpecificContent,en-US}',
        COALESCE(tenant_attr->'languageSpecificContent'->'en-US', '{}'::jsonb)
        || jsonb_build_object(
             'kpRedirectLink',  'https://healthy.kaiserpermanente.org/colorado/secure/inner-door',
             'kpGaRedirectLink',''
           ),
        true
      )
    WHERE tenant_attr ? 'languageSpecificContent'
      AND tenant_code = v_tenant_code;
END$$;
