-- Turn off Chat feature in INTEG KP Tenant
-- Execute this script only in Integ environment for KP tenant

DO
$$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';
BEGIN
    -- Update tenant_attr to set enableLiveChatbot to false
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
        tenant_attr::jsonb,
        '{liveChatbotInfo,enableLiveChatbot}',
        'false'::jsonb,
        true
    )
    WHERE tenant_attr::jsonb -> 'liveChatbotInfo' ->> 'enableLiveChatbot' = 'true'
    AND tenant_code = v_tenant_code;
END;
$$;