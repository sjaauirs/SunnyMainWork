DO $$
DECLARE
    env TEXT := 'Development';  -- Set to: Development, Qa, Uat, Integ, Production
    apply_to_all_tenants BOOLEAN := TRUE;  -- TRUE to apply auth_config_json to all tenants, FALSE for specific tenant_codes
--- pass tenant_codes empty for if apply_to_all_tenants is true 
    tenant_codes TEXT[] := ARRAY[
        'tenant_a',
        'tenant_b'
    ];
--- pass external_jwt_tenant_codes empty for if not required 

    external_jwt_tenant_codes TEXT[] := ARRAY[
        'tenant_c',
        'tenant_d'
    ];

    auth_config_json JSONB;
    external_jwt_auth_config_json JSONB;
BEGIN
    -- Set JSON configs based on environment
    IF env = 'Development' OR env = 'Qa' OR env = 'Uat' OR env = 'Integ' OR env = 'Newdev' THEN
        auth_config_json := '{
          "auth0": {
            "grantType": "client_credentials",
            "auth0VerificationEmailUrl": "https://dev-sunny-benefits.us.auth0.com/api/v2/jobs/verification-email",
            "auth0ApiUrl": "https://dev-sunny-benefits.us.auth0.com/api/v2/users/",
            "auth0UserInfoUrl": "https://dev-sunny-benefits.us.auth0.com/userinfo/",
            "auth0UserInfoByEmailUrl": "https://dev-sunny-benefits.us.auth0.com/api/v2/users-by-email?email=",
            "domain": "dev-sunny-benefits.us.auth0.com",
            "auth0TokenUrl": "https://dev-sunny-benefits.us.auth0.com/oauth/token",
            "audience": [
              "https://dev-sunny-benefits.us.auth0.com/api/v2/"
            ],
            "issuer": "https://dev-sunny-benefits.us.auth0.com/",
            "jwksUrl": "https://dev-sunny-benefits.us.auth0.com/.well-known/jwks.json"
          }
        }'::jsonb;

        external_jwt_auth_config_json := '{
          "auth0": {
            "audience": [
              "https://contact-center-dev-sunny.us.auth0.com/api/v2/",
              "https://contact-center-dev-sunny.us.auth0.com/userinfo"
            ],
            "issuer": "https://contact-center-dev-sunny.us.auth0.com/",
            "jwksUrl": "https://contact-center-dev-sunny.us.auth0.com/.well-known/jwks.json"
          }
        }'::jsonb;

    ELSIF env = 'Production' THEN
        auth_config_json := '{
          "auth0": {
            "grantType": "client_credentials",
            "auth0VerificationEmailUrl": "https://sunny-benefits.us.auth0.com/api/v2/jobs/verification-email",
            "auth0ApiUrl": "https://sunny-benefits.us.auth0.com/api/v2/users/",
            "auth0UserInfoUrl": "https://sunny-benefits.us.auth0.com/userinfo/",
            "auth0UserInfoByEmailUrl": "https://sunny-benefits.us.auth0.com/api/v2/users-by-email?email=",
            "domain": "sunny-benefits.us.auth0.com",
            "auth0TokenUrl": "https://sunny-benefits.us.auth0.com/oauth/token",
            "audience": [
              "https://sunny-benefits.us.auth0.com/api/v2/"
            ],
            "issuer": "https://sunny-benefits.us.auth0.com/",
            "jwksUrl": "https://sunny-benefits.us.auth0.com/.well-known/jwks.json"
          }
        }'::jsonb;

        external_jwt_auth_config_json := '{
          "auth0": {
            "audience": [
              "https://contact-center-sunny.us.auth0.com/api/v2/",
              "https://contact-center-sunny.us.auth0.com/userinfo"
            ],
            "issuer": "https://contact-center-sunny.us.auth0.com/",
            "jwksUrl": "https://contact-center-sunny.us.auth0.com/.well-known/jwks.json"
          }
        }'::jsonb;
    END IF;

    -- Apply auth_config_json to all or specific tenants
    IF apply_to_all_tenants THEN
        UPDATE tenant.tenant
        SET auth_config = auth_config_json;
    ELSE
        UPDATE tenant.tenant
        SET auth_config = auth_config_json
        WHERE tenant_code = ANY(tenant_codes);
    END IF;

    -- Apply external_jwt_auth_config_json only if external_jwt_tenant_codes is not empty
    IF array_length(external_jwt_tenant_codes, 1) IS NOT NULL THEN
        UPDATE tenant.tenant
        SET auth_config = external_jwt_auth_config_json
        WHERE tenant_code = ANY(external_jwt_tenant_codes);
    END IF;
END $$;
