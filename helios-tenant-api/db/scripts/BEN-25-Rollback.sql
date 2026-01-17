-- Update activeTabBgColor and strokeEarnedColor for HAP
DO $$
DECLARE
  v_tenant_code text := '<HAP-TENANT-CODE>';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr =
      jsonb_set(
        jsonb_set(
          tenant_attr,
          '{ux,taskTileColors,activeTabBgColor}', '"#0078B3"', true
        ),
        '{ux,walletColors,strokeEarnedColor}', '"#57A635"', true
      )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;