-- ====================================================================================
-- Script: Add contentBgColor under ux -> taskTileColors in tenant_attr JSONB column
-- Description: Sets "contentBgColor": "#5F6062" for tenants with valid ux config
-- Target Table: tenant.tenant
-- ====================================================================================

UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    tenant_attr,
    '{ux,taskTileColors,contentBgColor}',
    '"#5F6062"',
    true
)
WHERE delete_nbr = 0
  AND tenant_attr IS NOT NULL
  AND tenant_attr->'ux' IS NOT NULL
  AND tenant_attr->'ux' != '{}'::jsonb;
