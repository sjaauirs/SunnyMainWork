-- Rollback tenant_attr JSON changes to remove "supportLiveTransferWhileProcessingNonMonetary"
UPDATE tenant.tenant
SET tenant_attr = tenant_attr - 'supportLiveTransferWhileProcessingNonMonetary'
WHERE tenant_attr IS NOT NULL
AND tenant_attr <> '{}'::jsonb 
AND delete_nbr = 0;