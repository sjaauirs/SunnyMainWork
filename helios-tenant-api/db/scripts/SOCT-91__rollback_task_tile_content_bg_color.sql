-- ====================================================================================
-- Rollback Script: Remove contentBgColor under ux -> taskTileColors from tenant_attr
-- Description: Unsets the added JSONB key to revert to previous state
-- Target Table: tenant.tenant
-- ====================================================================================

UPDATE tenant.tenant
SET tenant_attr = tenant_attr #- '{ux,taskTileColors,contentBgColor}'
WHERE delete_nbr = 0
  AND tenant_attr IS NOT NULL
  AND tenant_attr->'ux' IS NOT NULL
  AND tenant_attr->'ux'->'taskTileColors' IS NOT NULL
  AND tenant_attr->'ux'->'taskTileColors' ? 'contentBgColor';
