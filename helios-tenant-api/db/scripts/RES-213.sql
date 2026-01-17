
-----This is for all tenant
DO $$
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      tenant_attr::jsonb, 
                      '{ux,taskTileColors}', 
                      (
                        COALESCE(tenant_attr::jsonb #> '{ux,taskTileColors}', '{}'::jsonb) || 
                        jsonb_build_object(
                          'enrollmentButtonBgColor', '#0078B3',
                          'enrollmentButtonTextColor', '#FFFFFF',
                          'enrollmentButtonBorderColor', '#0078B3'
                        )
                      ),
                      false
                   )
  WHERE delete_nbr = 0
    AND tenant_attr IS NOT NULL;
END $$;


-------- this is for HAP tenant
DO $$
DECLARE
  v_tenant_code TEXT := 'HAP-tenant-Code';  -- <-- CHANGE tenant-code
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      jsonb_set(
                        tenant_attr::jsonb,
                        '{ux,taskTileColors}',
                        (
                          COALESCE(tenant_attr::jsonb #> '{ux,taskTileColors}', '{}'::jsonb) ||
                          jsonb_build_object(
                            'enrollmentButtonBgColor', '#FFFFFF',
                            'enrollmentButtonTextColor', '#181D27',
                            'enrollmentButtonBorderColor', '#181D27',
                            'inProgressTextFgColor', '#45474D'

                          )
                        )::jsonb,
                        true
                      ),
                      '{ux,themeColors,headerBgColor}',
                      '"#181D27"',
                      true
                    )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0
    AND tenant_attr IS NOT NULL;
END $$;


-------- this is for Watco tenant
DO $$
DECLARE
  v_tenant_code TEXT := 'Watco-tenant-Code';  -- <-- CHANGE tenant-code
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      tenant_attr::jsonb, 
                      '{ux,taskTileColors}', 
                      (
                        COALESCE(tenant_attr::jsonb #> '{ux,taskTileColors}', '{}'::jsonb) || 
                        jsonb_build_object(
                          'enrollmentButtonBgColor', '#FFC907',
                          'enrollmentButtonTextColor', '#000000',
                          'enrollmentButtonBorderColor', '#FFC907'
                        )
                      ),
                      false
                   )
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0
    AND tenant_attr IS NOT NULL;
END $$;

