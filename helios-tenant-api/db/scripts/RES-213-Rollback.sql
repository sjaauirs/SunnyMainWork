DO $$
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr = jsonb_set(
                      tenant_attr::jsonb,
                      '{ux,taskTileColors}',
                      (
                        (tenant_attr::jsonb #> '{ux,taskTileColors}')
                        - 'enrollmentButtonBgColor'
                        - 'enrollmentButtonTextColor'
                        - 'enrollmentButtonBorderColor'
                      ),
                      false
                   )
  WHERE delete_nbr = 0;
END $$;
