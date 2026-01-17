-- Setting showCardCopyRightText to show copyright text in my card screen
-- HAP
DO $$
DECLARE
  v_tenant_code text := '<HAP-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr,
      '{showCardCopyRightText}',
      'false'::jsonb,     
      true               
  )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;

DO $$
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = COALESCE(tenant_attr, '{}'::jsonb)
        - 'displayMyCardEligibileComponent'
        - 'displayBancorpCopyright',
      update_user = 'SYSTEM',
      update_ts = NOW()
  WHERE delete_nbr = 0;

  RAISE NOTICE 'Removed flags from %% tenants', FOUND;
END $$;

-- Navitus
DO $$
DECLARE
  v_tenant_code text := '<NAVITUS-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr,
      '{showCardCopyRightText}',
      'true'::jsonb,     
      true              
  )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;

-- Sunny
DO $$
DECLARE
  v_tenant_code text := '<SUNNY-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr,
      '{showCardCopyRightText}',
      'true'::jsonb,     
      true               
  )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;

-- KP
DO $$
DECLARE
  v_tenant_code text := '<KP-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
      tenant_attr,
      '{showCardCopyRightText}',
      'true'::jsonb,     
      true               
  )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;
