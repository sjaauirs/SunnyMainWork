-- This script adds the "benefitsOptions" property to the tenant_options_json column in the tenant.tenant table.
-- The new property will be:
-- {
--   "benefitsOptions": {
--     "disableOnboardingFlow": false
--   }
-- }
UPDATE tenant.tenant
SET tenant_option_json = jsonb_set(
    tenant_option_json, 
    '{benefitsOptions}', 
    COALESCE(tenant_option_json->'benefitsOptions', '{}'::jsonb) || '{"disableOnboardingFlow": false}'::jsonb, 
    true -- Ensure the key is created if it does not exist
)
WHERE tenant_option_json IS NOT NULL
AND tenant_option_json <> '{}'::jsonb 
AND delete_nbr = 0;

