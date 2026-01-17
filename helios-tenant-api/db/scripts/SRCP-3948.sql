UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    COALESCE(tenant_attr, '{}'::jsonb),  
    '{consumerWallet}', 
    COALESCE(tenant_attr->'consumerWallet', '{}'::jsonb) || '{"individualWallet": false}'::jsonb,  -- Handle missing consumerWallet
    true
) 
WHERE COALESCE(Delete_nbr, 0) = 0;  



  UPDATE tenant.tenant
SET tenant_attr = jsonb_set(
    COALESCE(tenant_attr, '{}'::jsonb),  
    '{consumerWallet}', 
    COALESCE(tenant_attr->'consumerWallet', '{}'::jsonb) || '{"contributorMaximum": 250}'::jsonb,  -- Handle missing consumerWallet
    true
) 
WHERE COALESCE(Delete_nbr, 0) = 0;