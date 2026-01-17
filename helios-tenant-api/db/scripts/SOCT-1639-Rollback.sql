-- SOCT-1639
-- ROllback Update en-US dataDeleteLink 
DO
$$
DECLARE
    v_tenant_code TEXT := '<KP_TENANT_CODE>';
    v_new_url_en  TEXT := 'https://espanol.kaiserpermanente.org/es/front-door';
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
        tenant_attr::jsonb,
        '{languageSpecificContent,en-US,dataDeleteLink}',
        to_jsonb(v_new_url_en),   -- <-- JSON string, not plain text
        true
    )
    WHERE tenant_code = v_tenant_code
      AND tenant_attr::jsonb #>> '{languageSpecificContent,en-US,dataDeleteLink}'
          = 'https://healthy.kaiserpermanente.org/privacy';
END;
$$;