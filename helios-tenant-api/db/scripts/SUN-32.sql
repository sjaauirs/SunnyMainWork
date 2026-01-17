-- Add 'completionToggleAvailableActions' flag to root level of tenant_attr
BEGIN;

UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    tenant_attr,
    '{completionToggleAvailableActions}',
    'true'::jsonb,
    true
)
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb
  AND NOT tenant_attr ? 'completionToggleAvailableActions'
  AND delete_nbr = 0;

COMMIT;