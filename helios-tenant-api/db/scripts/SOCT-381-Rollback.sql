-- Rollback: Remove 'autoCompleteTaskOnLogin' Flag to 'benefitOptions'
BEGIN;

UPDATE tenant.tenant
SET tenant_option_json = jsonb_set(
    tenant_option_json,
    '{benefitsOptions}',
    (tenant_option_json->'benefitsOptions') - 'autoCompleteTaskOnLogin',
    true
)
WHERE tenant_option_json IS NOT NULL
  AND tenant_option_json <> '{}'::jsonb
  AND tenant_option_json ? 'benefitsOptions'
  AND tenant_option_json->'benefitsOptions' ? 'autoCompleteTaskOnLogin'
  AND delete_nbr = 0;

COMMIT;