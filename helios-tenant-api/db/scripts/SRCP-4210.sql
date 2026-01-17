UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    -- First jsonb_set: Update 'completedTileBgColor' within the 'ux,taskTileColors' path to the new color '#868C92'
    jsonb_set(tenant_attr, '{ux,taskTileColors,completedTileBgColor}', '"#868C92"'::jsonb),
    
    -- Second jsonb_set: Update 'activeTabBgColor' within the 'ux,taskTileColors' path to the new color '#0078B3'
    '{ux,taskTileColors,activeTabBgColor}', '"#0078B3"'::jsonb
)
WHERE tenant_attr IS NOT NULL  -- Ensure the 'tenant_attr' column is not NULL
  AND tenant_attr <> '{}'::jsonb  -- Ensure 'tenant_attr' is not an empty JSON object
  AND delete_nbr = 0;  -- Only update records where 'delete_nbr' is 0
