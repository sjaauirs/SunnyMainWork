DO $$
DECLARE
  v_tenant_code text := '<KP-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr =
    jsonb_set(
      jsonb_set(
        tenant_attr,
        '{languageSpecificContent,es}',
        COALESCE(tenant_attr->'languageSpecificContent'->'es','{}'::jsonb)
          - 'kpRedirectLink' - 'kpGaRedirectLink',
        true
      ),
      '{languageSpecificContent,en-US}',
      COALESCE(tenant_attr->'languageSpecificContent'->'en-US','{}'::jsonb)
        - 'kpRedirectLink' - 'kpGaRedirectLink',
      true
    )
  WHERE tenant_code = v_tenant_code
    AND tenant_attr ? 'languageSpecificContent';
END$$;
