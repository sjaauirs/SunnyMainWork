-- Add 'autoCompleteTaskOnLogin' Flag to 'benefitOptions'
BEGIN;

UPDATE tenant.tenant
SET tenant_option_json = jsonb_set(
    tenant_option_json,
    '{benefitsOptions,autoCompleteTaskOnLogin}',
    'false'::jsonb,
    true
)
WHERE tenant_option_json IS NOT NULL
  AND tenant_option_json <> '{}'::jsonb
  AND tenant_option_json ? 'benefitsOptions'
  AND NOT (tenant_option_json->'benefitsOptions') ? 'autoCompleteTaskOnLogin'
  AND delete_nbr = 0;

COMMIT;