UPDATE tenant.tenant
SET tenant_attr = COALESCE(
    -- Attempt to update the 'ux,taskTileColors' key by removing 'completedTileBgColor' and 'activeTabBgColor'
    jsonb_set(
        tenant_attr, 
        '{ux,taskTileColors}', 
        (tenant_attr #> '{ux,taskTileColors}')::jsonb - 'completedTileBgColor' - 'activeTabBgColor'
    ),
    -- If jsonb_set returns NULL (i.e., if 'ux,taskTileColors' doesn't exist), keep the existing 'tenant_attr' value
    tenant_attr  
)
WHERE tenant_attr IS NOT NULL  -- Ensure the column is not NULL
  AND tenant_attr <> '{}'::jsonb  -- Ensure the column is not an empty JSON object
  AND delete_nbr = 0;  -- Only update records where 'delete_nbr' is 0
