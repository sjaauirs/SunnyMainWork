-- This script updates the tenant_attr JSON column in the tenant.tenant table to add the property "costcoMembershipSupport": false 
UPDATE tenant.tenant
SET tenant_attr = jsonb_set(tenant_attr, '{costcoMembershipSupport}', 'false'::jsonb, true)
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb 
  AND delete_nbr=0;


-- Rollback tenant_attr JSON changes to remove "costcoMembershipSupport"
/*
UPDATE tenant.tenant
SET tenant_attr = tenant_attr - 'costcoMembershipSupport'
WHERE delete_nbr = 0;
*/
