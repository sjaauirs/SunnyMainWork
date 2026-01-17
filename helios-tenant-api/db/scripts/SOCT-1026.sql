--Update tenant attributes josn to add supportLiveTransferWhileProcessingNonMonetary property 

UPDATE tenant.tenant
SET tenant_attr =  jsonb_set(tenant_attr, '{supportLiveTransferWhileProcessingNonMonetary}', 'false'::jsonb, true)
WHERE tenant_attr IS NOT NULL
AND tenant_attr <> '{}'::jsonb 
AND delete_nbr = 0;