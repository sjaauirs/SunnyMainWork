-- This script updates the tenant_attr JSON column in the tenant.tenant table to add the property "maxAllowedPickAPurseSelection": 2 for a specific tenant
UPDATE tenant.tenant 
SET tenant_attr = jsonb_set(tenant_attr, '{maxAllowedPickAPurseSelection}', '2'::jsonb) 
WHERE tenant_code = 'ten-ecada21e57154928a2bb959e8365b8b4' 
AND delete_nbr = 0;