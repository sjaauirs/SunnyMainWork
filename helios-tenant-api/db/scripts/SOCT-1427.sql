DO $$
DECLARE
    tenantCode TEXT := '<Wotco>'; -- Update tenant code here
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json::jsonb, 
        '{benefitsOptions}', 
        (tenant_option_json->'benefitsOptions') || 
        '{"hamburgerMenu": ["myRewards", "personal", "help", "officialRules", "privacyPolicy"]}'::jsonb
    )
    WHERE tenant_code = tenantCode
      AND tenant_option_json IS NOT NULL
      AND tenant_option_json <> '{}'::jsonb 
      AND tenant_option_json::jsonb ? 'benefitsOptions'
	  AND delete_nbr = 0;
END $$;



DO $$
DECLARE
    tenantCode TEXT := '<KPTenantCode>'; -- Update tenant code here
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json::jsonb, 
        '{benefitsOptions}', 
        (tenant_option_json->'benefitsOptions') || 
        '{"hamburgerMenu": ["myCard", "myRewards", "healthAdventures", "personal", "notifications", "manageCard", "security", "help", "agreements", "privacyPolicy", "signOut"]}'::jsonb
    )
    WHERE tenant_code = tenantCode
      AND tenant_option_json IS NOT NULL
      AND tenant_option_json <> '{}'::jsonb 
      AND tenant_option_json::jsonb ? 'benefitsOptions'
	  AND delete_nbr = 0;
END $$;



DO $$
DECLARE
    tenantCode TEXT := '<SunnyTenantCode>'; -- Update tenant code here
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json::jsonb, 
        '{benefitsOptions}', 
        (tenant_option_json->'benefitsOptions') || 
        '{"hamburgerMenu": ["personal",  "manageCard", "privacyPolicy", "signOut"]}'::jsonb
    )
    WHERE tenant_code = tenantCode
      AND tenant_option_json IS NOT NULL
      AND tenant_option_json <> '{}'::jsonb 
      AND tenant_option_json::jsonb ? 'benefitsOptions'
	  AND delete_nbr = 0;
END $$;


DO $$
DECLARE
    tenantCode TEXT := '<Navitas>'; -- Update tenant code here
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json::jsonb, 
        '{benefitsOptions}', 
        (tenant_option_json->'benefitsOptions') || 
        '{"hamburgerMenu": ["personal",  "manageCard", "privacyPolicy", "signOut"]}'::jsonb
    )
    WHERE tenant_code = tenantCode
      AND tenant_option_json IS NOT NULL
      AND tenant_option_json <> '{}'::jsonb 
      AND tenant_option_json::jsonb ? 'benefitsOptions'
	  AND delete_nbr = 0;
END $$;
