-- =====================================================================
-- Script : Update tenant_attr with agreementColors and agreementDeclineImageUrl (Idempotent + Rename if exists)
-- Author : Pernati Rakesh
-- Purpose: 
--   - If agreementsColors exists → rename to agreementColors and update values
--   - If not exists → insert agreementColors
--   - Add agreementDeclineImageUrl if missing
--   - Logs actions for clarity
-- JIRA Ticket : BEN-6
-- =====================================================================

DO $$
DECLARE
    v_env TEXT := '<ENVIRONMENT>';         -- Replace with: DEV / QA / UAT / INTEG / PROD
    v_env_specific_url TEXT;

    hap_tenant_code TEXT := '<HAP-TENANT-CODE>';    -- input 1
    other_tenant_codes TEXT[] := ARRAY['<KP-TENANT-CODE>', '<WATCO-TENANT-CODE>','NAVITUS-TENANT-CODE'];      -- input 2

    hap_decline_url TEXT;
    other_decline_url TEXT;
    v_exists BOOLEAN;
BEGIN
    -- Resolve environment-specific base URL
    CASE v_env
        WHEN 'DEV'   THEN v_env_specific_url := 'https://app-static.dev.sunnyrewards.com';
        WHEN 'QA'    THEN v_env_specific_url := 'https://app-static.qa.sunnyrewards.com';
        WHEN 'UAT'   THEN v_env_specific_url := 'https://app-static.uat.sunnyrewards.com';
        WHEN 'INTEG' THEN v_env_specific_url := 'https://app-static.integ.sunnyrewards.com';
        WHEN 'PROD'  THEN v_env_specific_url := 'https://app-static.sunnyrewards.com';
        ELSE
            RAISE EXCEPTION 'Invalid environment [%]. Please choose from DEV, QA, UAT, INTEG, PROD.', v_env;
    END CASE;

    hap_decline_url   := v_env_specific_url || '/public/images/hap_agreement_decline_image.png';
    other_decline_url := v_env_specific_url || '/public/images/agreement_decline_image.png';

    -- =====================================================================
    -- HAP tenant updates
    -- =====================================================================
    -- Add agreementDeclineImageUrl if missing
    SELECT t.tenant_attr ? 'agreementDeclineImageUrl'
    INTO v_exists
    FROM tenant.tenant t
    WHERE tenant_code = hap_tenant_code AND delete_nbr = 0;

    IF NOT v_exists THEN
        UPDATE tenant.tenant
        SET tenant_attr = tenant_attr || jsonb_build_object('agreementDeclineImageUrl', hap_decline_url)
        WHERE tenant_code = hap_tenant_code AND delete_nbr = 0;
        RAISE NOTICE '[HAP] Added agreementDeclineImageUrl for tenant %', hap_tenant_code;
    ELSE
        RAISE WARNING '[HAP] agreementDeclineImageUrl already exists for tenant %, skipped', hap_tenant_code;
    END IF;

    -- Handle agreementsColors → agreementColors for HAP
    SELECT (tenant_attr #> '{ux,agreementsColors}') IS NOT NULL
    INTO v_exists
    FROM tenant.tenant
    WHERE tenant_code = hap_tenant_code AND delete_nbr = 0;

    IF v_exists THEN
        -- Rename key and update colors
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
                            tenant_attr - 'ux' || jsonb_build_object(
                                'ux',
                                (tenant_attr->'ux') - 'agreementsColors' || jsonb_build_object(
                                    'agreementColors',
                                    '{
                                        "agreeButtonColor": "#181D27",
                                        "agreeButtonLabelColor": "#FFFFFF",
                                        "declineButtonLabelColor": "#181D27"
                                    }'::jsonb
                                )
                            ),
                            '{}',
                            (tenant_attr)::jsonb,
                            TRUE
                          )
        WHERE tenant_code = hap_tenant_code AND delete_nbr = 0;
        RAISE NOTICE '[HAP] Renamed agreementsColors -> agreementColors and updated colors for tenant %', hap_tenant_code;
    ELSE
        -- Insert agreementColors if missing
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
                            tenant_attr,
                            '{ux,agreementColors}',
                            '{
                                "agreeButtonColor": "#181D27",
                                "agreeButtonLabelColor": "#FFFFFF",
                                "declineButtonLabelColor": "#181D27"
                            }'::jsonb,
                            TRUE
                          )
        WHERE tenant_code = hap_tenant_code AND delete_nbr = 0;
        RAISE NOTICE '[HAP] Inserted agreementColors for tenant %', hap_tenant_code;
    END IF;

    -- =====================================================================
    -- Other tenants updates
    -- =====================================================================
    FOREACH hap_tenant_code IN ARRAY other_tenant_codes
    LOOP
        -- Add agreementDeclineImageUrl if missing
        SELECT t.tenant_attr ? 'agreementDeclineImageUrl'
        INTO v_exists
        FROM tenant.tenant t
        WHERE tenant_code = hap_tenant_code AND delete_nbr = 0;

        IF NOT v_exists THEN
            UPDATE tenant.tenant
            SET tenant_attr = tenant_attr || jsonb_build_object('agreementDeclineImageUrl', other_decline_url)
            WHERE tenant_code = hap_tenant_code AND delete_nbr = 0;
            RAISE NOTICE '[OTHER] Added agreementDeclineImageUrl for tenant %', hap_tenant_code;
        ELSE
            RAISE WARNING '[OTHER] agreementDeclineImageUrl already exists for tenant %, skipped', hap_tenant_code;
        END IF;

        -- Handle agreementsColors → agreementColors
        SELECT (tenant_attr #> '{ux,agreementsColors}') IS NOT NULL
        INTO v_exists
        FROM tenant.tenant
        WHERE tenant_code = hap_tenant_code AND delete_nbr = 0;

        IF v_exists THEN
            -- Rename key and update colors
            UPDATE tenant.tenant
            SET tenant_attr = jsonb_set(
                                tenant_attr - 'ux' || jsonb_build_object(
                                    'ux',
                                    (tenant_attr->'ux') - 'agreementsColors' || jsonb_build_object(
                                        'agreementColors',
                                        '{
                                            "agreeButtonColor": "#0078B3",
                                            "agreeButtonLabelColor": "#FFFFFF",
                                            "declineButtonColor": "transparent",
                                            "declineButtonLabelColor": "#0078B3"
                                        }'::jsonb
                                    )
                                ),
                                '{}',
                                (tenant_attr)::jsonb,
                                TRUE
                              )
            WHERE tenant_code = hap_tenant_code AND delete_nbr = 0;
            RAISE NOTICE '[OTHER] Renamed agreementsColors -> agreementColors and updated colors for tenant %', hap_tenant_code;
        ELSE
            -- Insert if missing
            UPDATE tenant.tenant
            SET tenant_attr = jsonb_set(
                                tenant_attr,
                                '{ux,agreementColors}',
                                '{
                                    "agreeButtonColor": "#0078B3",
                                    "agreeButtonLabelColor": "#FFFFFF",
                                    "declineButtonColor": "transparent",
                                    "declineButtonLabelColor": "#0078B3"
                                }'::jsonb,
                                TRUE
                              )
            WHERE tenant_code = hap_tenant_code AND delete_nbr = 0;
            RAISE NOTICE '[OTHER] Inserted agreementColors for tenant %', hap_tenant_code;
        END IF;
    END LOOP;
END $$;
