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
      'true'::jsonb,     
      true               
  )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;

DO
$$
DECLARE
    v_user_id TEXT := 'SYSTEM';
    v_now TIMESTAMP := NOW();
    -- List of tenants where flags should be TRUE
    v_target_tenant_codes TEXT[] := ARRAY[
        '<HAP-TENANT-CODE>'  -- add more tenant codes here
    ];
BEGIN
    -- 1. Update target tenants (flags = true)
    UPDATE tenant.tenant
    SET tenant_attr = COALESCE(tenant_attr, '{}'::JSONB)
        || jsonb_build_object(
            'displayMyCardEligibileComponent', true,
            'displayBancorpCopyright', true
        ),
        update_user = v_user_id,
        update_ts = v_now
    WHERE tenant_code = ANY(v_target_tenant_codes)
      AND delete_nbr = 0;

    RAISE NOTICE 'Updated target tenants to TRUE flags.', FOUND;

    -- 2. Update all other active tenants (flags = false)
    UPDATE tenant.tenant
    SET tenant_attr = COALESCE(tenant_attr, '{}'::JSONB)
        || jsonb_build_object(
            'displayMyCardEligibileComponent', false,
            'displayBancorpCopyright', false
        ),
        update_user = v_user_id,
        update_ts = v_now
    WHERE tenant_code <> ALL(v_target_tenant_codes)
      AND delete_nbr = 0;

    RAISE NOTICE 'Updated non-target active tenants to FALSE flags.', FOUND;
END
$$;

-- Navitus
DO $$
DECLARE
  v_tenant_code text := '<NAVITUS-TENANT-CODE>';
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

-- Sunny
DO $$
DECLARE
  v_tenant_code text := '<SUNNY-TENANT-CODE>';
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

-- KP
DO $$
DECLARE
  v_tenant_code text := '<KP-TENANT-CODE>';
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
