-- This script is used to add the "membershipWallet" object to the tenant_attr
UPDATE tenant.tenant
SET tenant_attr = jsonb_set(tenant_attr, '{membershipWallet}', '{"earnMaximum": 500}'::jsonb, true)
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb 
  AND delete_nbr = 0;