DO $$
DECLARE
    tenantCode TEXT := '<SunnyTenantCode>'; -- Update tenant code here
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json::jsonb, 
        '{benefitsOptions}', 
        (tenant_option_json->'benefitsOptions') || 
        '{"hamburgerMenu": ["myCard","myRewards" ,"personal",  "manageCard", "privacyPolicy", "signOut"]}'::jsonb
    )
    WHERE tenant_code = tenantCode
      AND tenant_option_json IS NOT NULL
      AND tenant_option_json <> '{}'::jsonb 
      AND tenant_option_json::jsonb ? 'benefitsOptions'
	  AND delete_nbr = 0;
END $$;
