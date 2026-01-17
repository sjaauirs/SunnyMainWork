--Update tenant attributes josn to add walletSplitEnabled property 

UPDATE tenant.tenant
SET tenant_attr =  jsonb_set(tenant_attr, '{walletSplitEnabled}', 'false'::jsonb, true)
WHERE tenant_attr IS NOT NULL
AND tenant_attr <> '{}'::jsonb 
AND delete_nbr = 0;