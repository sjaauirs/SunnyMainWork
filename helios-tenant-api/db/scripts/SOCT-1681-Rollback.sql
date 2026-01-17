-- Rollback: remove triviaProgressBarColor key
DO
$$
DECLARE
   v_tenant_code TEXT := '<WATCO-TENANT-CODE>';
BEGIN
   UPDATE tenant.tenant
   SET tenant_attr = tenant_attr::jsonb - '{ux,themeColors,triviaProgressBarColor}'
   WHERE tenant_code = v_tenant_code;
END;
$$;
