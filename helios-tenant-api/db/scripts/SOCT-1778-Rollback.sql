DO $$
DECLARE
  v_tenant_code text := '<KP-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr,
      '{displayBancorpCopyright}',
      'false'::jsonb,     
      true               
  )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;