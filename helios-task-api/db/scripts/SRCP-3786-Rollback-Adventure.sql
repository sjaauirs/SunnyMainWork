-- Rollback script to task.adventure

DELETE FROM task.adventure 
WHERE cms_component_code in (SELECT component_code FROM cms.component WHERE component_name in ('healthy-eating','fitness-and-exercise','mental-and-wellness') 
AND tenant_code='ten-ecada21e57154928a2bb959e8365b8b4' AND delete_nbr=0)
AND delete_nbr =0 ;