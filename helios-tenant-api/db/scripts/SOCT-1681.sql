-- Update triviaProgressBarColor
DO
$$
DECLARE
   v_tenant_code TEXT := '<WATCO-TENANT-CODE>';
   v_new_value  TEXT := '#FEC90B';
BEGIN
   UPDATE tenant.tenant
   SET tenant_attr = jsonb_set(
       tenant_attr::jsonb,
       '{ux,themeColors,triviaProgressBarColor}',
       to_jsonb(v_new_value), 
       true
   )
   WHERE tenant_code = v_tenant_code;
END;
$$;