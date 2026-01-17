-- Update the tenant attribute JSON to include the "spendOnGiftCardDisabled" flag.
-- This ensures that all valid tenants have the "spendOnGiftCardDisabled" property set to false.
UPDATE tenant.tenant
SET tenant_attr = jsonb_set(tenant_attr, '{spendOnGiftCardDisabled}', 'false'::jsonb, true)
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb 
  AND delete_nbr = 0;
