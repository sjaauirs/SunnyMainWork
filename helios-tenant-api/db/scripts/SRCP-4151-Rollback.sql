
-- This script reverts the changes made by `SRCP-4151_update_tenant_attr_ux.sql`. It removes the `taskTileColors`, `taskTileColors` 
-- and `entriesWalletColors` properties from the `ux` object in the `tenant_attr` column while preserving
-- other existing `ux` properties.
UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    tenant_attr, 
    '{ux}', 
    (tenant_attr->'ux') - 'walletColors' - 'entriesWalletColors' - 'taskTileColors',
    true
)
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb 
  AND tenant_attr ? 'ux'
  AND delete_nbr = 0;