-- This script updates the `startPage` attribute in the `tenant_attr` column of the `tenant.tenant` table.  
-- The `startPage` value is determined based on the `apps` array in the `tenant_options_json` column:  
-- - If "BENEFITS" is present in the `apps` array, `startPage` is set to 'BENEFITS-HOME'.  
-- - Otherwise, `startPage` is set to 'REWARDS-HOME'.  
-- The update is applied only to non-null and non-empty `tenant_attr` records where `delete_nbr = 0`.  

UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    tenant_attr, 
    '{startPage}', 
    CASE 
        WHEN tenant_option_json->'apps' ? 'BENEFITS' 
        THEN '"BENEFITS-HOME"'::jsonb
        ELSE '"REWARDS-HOME"'::jsonb
    END
)
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb 
  AND delete_nbr = 0;
