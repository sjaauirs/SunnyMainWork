
--Update tenant master -- Add 

UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    jsonb_set(tenant_attr, '{adventuresEnabled}', 'false'::jsonb, true), 
    '{adventureEarnMaximum}', '100'::jsonb, true
)
WHERE tenant_attr IS NOT NULL
AND tenant_attr <> '{}'::jsonb 
AND delete_nbr = 0;

