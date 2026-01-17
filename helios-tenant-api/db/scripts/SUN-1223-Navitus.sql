-- ============================================================================
-- 🚀 Script    : Fix tenant_attr properties for Navitus
-- 📌 Purpose   : Fix tenant_attr properties for Navitus
-- 👨‍💻 Author    : Neel
-- 📅 Date      : 2025-12-05
-- 🧾 Jira      : SUN-1223
-- ⚠️ Inputs    : v_tenant_codes[]
-- 📤 Output    : Updates tenant_attr JSON structure
-- 📝 Notes     :
--    - Adds or overwrites enableStoresSection in tenant_attr.
--    - Idempotent: safe to run multiple times.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant TEXT;
	v_env TEXT :='QA';
    v_env_specific_url TEXT;                
	v_trivia_mobile_image_url TEXT;
BEGIN
    -- Resolve environment-specific static URL
    CASE v_env
        WHEN 'DEV' THEN
            v_env_specific_url := 'https://app-static.dev.sunnyrewards.com';
        WHEN 'QA' THEN
            v_env_specific_url := 'https://app-static.qa.sunnyrewards.com';
        WHEN 'UAT' THEN
            v_env_specific_url := 'https://app-static.uat.sunnyrewards.com';
        WHEN 'INTEG' THEN
            v_env_specific_url := 'https://app-static.integ.sunnyrewards.com';
        WHEN 'PROD' THEN
            v_env_specific_url := 'https://app-static.sunnyrewards.com';
        ELSE
            RAISE EXCEPTION 'Invalid environment [%]. Choose from DEV, QA, UAT, INTEG, PROD.', v_env;
    END CASE;

    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP
        v_trivia_mobile_image_url = v_env_specific_url || '/cms/images/' || v_tenant || '/trivia_mobile_image.png';
        
        UPDATE tenant.tenant
        SET tenant_attr =
            jsonb_set(
            jsonb_set(
			jsonb_set(
            jsonb_set(
            jsonb_set(
            jsonb_set(
            jsonb_set(
            jsonb_set(
            jsonb_set(
            jsonb_set(
            jsonb_set(
            jsonb_set(
            jsonb_set(
            jsonb_set(
                -- 1. Root-level flags merged in (adds/overwrites only these keys)
                tenant_attr
                || jsonb_build_object(
                    'displayMobileHeader',                     true,
                    'hideNameInitials',                        true,
                    'enableStoresSection',                     false,
                    'displayBancorpCopyright',                 true,
                    -- NEW root-level properties
                    'isTermsAndConditionVisibleForOrderCard',  false,
                    -- If you really need this as a string "true",
                    -- change to 'true'::text instead of the boolean.
                    'displayAlternateDatePicker',              true
                )::jsonb,

                -- 2. ux.mycardColors.walletBgColor = "#FFFFFF"
                '{ux,mycardColors,walletBgColor}',
                to_jsonb('#FFFFFF'::text),
                true  -- create missing
            ),
            -- 3. tenantAttribute.ux.headerColors.headerBgColor = "#FFFFFF"
            '{tenantAttribute,ux,headerColors,headerBgColor}',
            to_jsonb('#FFFFFF'::text),
            true  -- create missing
            ),
            -- 4. ux.themeColors.headerBgColor = "#0B0C0E"
            '{ux,themeColors,headerBgColor}',
            to_jsonb('#0B0C0E'::text),
            true  -- create missing
            ),
            -- 5. top-level headerColors.headerBgColor = "#FFFFFF"
            '{ux,headerColors,headerBgColor}',
            to_jsonb('#FFFFFF'::text),
            true  -- create missing
            ),
            -- 6. commonColors.textColor7 = "#0B0C0E"
            '{ux,commonColors,textColor7}',
            to_jsonb('#0B0C0E'::text),
            true  -- create missing
            ),
            -- 7. commonColors.contentBgColor = "#FFFFFF"
            '{ux,commonColors,contentBgColor}',
            to_jsonb('#FFFFFF'::text),
            true  -- create missing
            ),
            -- 8. ux.themeColors.taskGradient1 = "#21495F"
            '{ux,themeColors,taskGradient1}',
            to_jsonb('#3B83AB'::text),
            true  -- create missing
            ),
            -- 9. ux.themeColors.taskGradient2 = "#3B83AB"
            '{ux,themeColors,taskGradient2}',
            to_jsonb('#21495F'::text),
            true  -- create missing
            ),
            -- 10. ux.taskTileColors.activeTabBgColor = "#21495F"
            '{ux,taskTileColors,activeTabBgColor}',
            to_jsonb('#21495F'::text),
            true  -- create missing
            ),
            -- 11. triviaColors.progressBarFillColor = "#E27025"
            '{ux,triviaColors,progressBarFillColor}',
            to_jsonb('#E27025'::text),
            true  -- create missing
            ),
            -- 12. commonColors.borderColor2 = "#E27025"
            '{ux,commonColors,borderColor2}',
            to_jsonb('#E27025'::text),
            true  -- create missing
            ),
            -- 13. triviaColors.triviaOptionsTextColor = "#0B0C0E"
            '{ux,triviaColors,triviaOptionsTextColor}',
            to_jsonb('#0B0C0E'::text),
            true  -- create missing
            ),
            -- 14. tenantAttribute.TriviaMobileImage (placeholder value)            
			'{triviaMobileImageUrl}',
			to_jsonb(v_trivia_mobile_image_url::text),
			true
            ),
            '{ux,cardActivationBannerColors,bannerTextColorOnActivation}',
            to_jsonb('#326F91'::text),
            true
            );

        -- finally update timestamp
        UPDATE tenant.tenant
        SET update_ts = NOW()
        WHERE tenant_code = v_tenant
          AND delete_nbr = 0;

        RAISE NOTICE '✅ Updated attributes for Navitus tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🏁 Completed updating tenant attributes for all NAVITUS tenants.';
END $$;
