
-- Update the tenant attribute JSON to include the "isHybridRewards" flag.
-- This ensures that all valid tenants have the "isHybridRewards" property set to false.
UPDATE tenant.tenant
SET tenant_attr = jsonb_set(tenant_attr, '{isHybridRewards}', 'false'::jsonb, true)
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb 
  AND delete_nbr = 0;
