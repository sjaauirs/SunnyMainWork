-- Rollback: Remove 'completionToggleAvailableActions' flag from root level of tenant_attr
BEGIN;

UPDATE tenant.tenant
SET tenant_attr = tenant_attr - 'completionToggleAvailableActions'
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb
  AND tenant_attr ? 'completionToggleAvailableActions'
  AND delete_nbr = 0;

COMMIT;