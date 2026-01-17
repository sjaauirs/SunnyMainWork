UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    tenant_attr, 
    '{consumerWallet}', 
    (tenant_attr->'consumerWallet') - 'splitRewardOverflow',
    true
) 
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb 
  AND delete_nbr = 0;
