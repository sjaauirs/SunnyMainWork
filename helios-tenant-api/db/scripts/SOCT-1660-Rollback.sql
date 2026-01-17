-- Remove CalendarSelectedDayColor, inProgressTextFgColor, TriviaDesktopImage, TriviaMobileImage
DO $$
DECLARE
  v_tenant_code text := 'TENANT-CODE';
BEGIN
  UPDATE tenant.tenant
  SET tenant_attr =
      (tenant_attr #- '{ux,themeColors,CalendarSelectedDayColor}')
      #- '{ux,taskTileColors,inProgressTextFgColor}'
      #- '{ux,TriviaDesktopImage}'
      #- '{ux,TriviaMobileImage}'
  WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;
END $$;
