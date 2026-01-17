DO $$
DECLARE
    tenantCode TEXT := 'sunnyrewards';  -- Replace with your actual tenant code
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json = jsonb_set(
        tenant_option_json::jsonb,
        '{benefitsOptions}',
        (tenant_option_json->'benefitsOptions') - 'hamburgerMenu'
    )
    WHERE tenant_code = tenantCode
      AND tenant_option_json IS NOT NULL
      AND tenant_option_json <> '{}'::jsonb
      AND tenant_option_json::jsonb ? 'benefitsOptions'
      AND tenant_option_json->'benefitsOptions' ? 'hamburgerMenu';
END $$;
