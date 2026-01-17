-- This is used to rollback SRCP-4060-add_benefits_options_to_tenant.sql changes
UPDATE tenant.tenant
SET tenant_option_json = jsonb_set(
    tenant_option_json, 
    '{benefitsOptions}', 
    (tenant_option_json->'benefitsOptions') - 'disableOnboardingFlow', 
    true
)
WHERE tenant_option_json IS NOT NULL
AND tenant_option_json <> '{}'::jsonb 
AND delete_nbr = 0;
