-- ==========================================================================================
-- 🚀 Script    : Create or Update UX Color Attributes in tenant_attr JSON
-- 📌 Purpose   : Ensures the UX color fields exist inside tenant_attr JSON for each tenant.
--                If a field exists → updates value.
--                If a field is missing → creates the field.
-- 👨‍💻 Author   : Srikanth Kodam
-- 📅 Date       : 2025-11-19
-- 🧾 Jira       : BEN-2010
-- ⚠️ Inputs     :
--      v_tenant_codes → List of tenant codes to update (<NAVITUS-TENANT-CODE>)
--		v_env →  change the <ENVIRONMENT> ex: (DEV,QA,UAT,INTEG,PROD)
-- 📤 Output     :
--      - Updates tenant_attr JSON in tenant.tenant table
--      - Logs status for every tenant
--      - Summary (processed, updated, errors)
-- 📝 Notes:
--      - Fully idempotent
--      - Uses jsonb_set() with create-missing = TRUE
-- ==========================================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        '<NAVITUS-TENANT-CODE>'
    ];
    v_env TEXT := '<ENVIRONMENT>';
    v_tenant_code TEXT;
    v_env_host TEXT;
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_mobileIconUrl TEXT;
    v_desktopIconUrl TEXT;
    v_updated BOOLEAN := false;
    v_total_processed INT := 0;
    v_total_updated   INT := 0;
    v_total_errors    INT := 0;

BEGIN
    -- Resolve base URL
    CASE v_env
        WHEN 'DEV'   THEN v_env_host := 'https://app-static.dev.sunnyrewards.com';
        WHEN 'QA'    THEN v_env_host := 'https://app-static.qa.sunnyrewards.com';
        WHEN 'UAT'   THEN v_env_host := 'https://app-static.uat.sunnyrewards.com';
        WHEN 'INTEG' THEN v_env_host := 'https://app-static.integ.sunnyrewards.com';
        WHEN 'PROD'  THEN v_env_host := 'https://app-static.sunnyrewards.com';
        ELSE RAISE EXCEPTION '❌ Invalid environment: %', v_env;
    END CASE;

    -- Loop tenants
    FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        v_total_processed := v_total_processed + 1;

        BEGIN
            RAISE NOTICE '➡ Processing tenant: %', v_tenant_code;

            -- Fetch existing tenant_attr
            SELECT tenant_attr INTO v_old_attr
            FROM tenant.tenant
            WHERE tenant_code = v_tenant_code
              AND delete_nbr = 0;

            IF NOT FOUND THEN
                RAISE WARNING '⚠️ Tenant not found → %', v_tenant_code;
                CONTINUE;
            END IF;

            v_new_attr := COALESCE(v_old_attr, '{}'::jsonb);
            v_updated := false;

            -- Build icon URLs
            v_mobileIconUrl  := rtrim(v_env_host, '/') || '/public/images/navitus_mobile_logo.png';
            v_desktopIconUrl := rtrim(v_env_host, '/') || '/public/images/navitus_desktop_logo.png';

            -- Consolidated update for ux.commonColors
            v_new_attr := jsonb_set(
                v_new_attr,
                '{ux,commonColors}',
                (
                    COALESCE(v_new_attr #> '{ux,commonColors}', '{}'::jsonb)
                    ||
                    jsonb_build_object(
                        'textColor', '#0B0C0E',
                        'screenBgColor', '#F7F7F7',
                        'screenTitleShadowColor', '#B1B2B4',
                        'buttonColor', '#E27025',
                        'button1Color', '#326F91',
                        'menuIconColor', '#0B0C0E',
                        'textColor6', '#C25700',
						'textColor3', '#0B0C0E',
                        'buttonTextColor2', '#0B0C0E',
                        'hyperLinkTextColor', '#A34608',
                        'contentBgColor', '#F7F7F7'
                    )
                ),
                true
            );

            -- ux.cardActivationBannerColors
            v_new_attr := jsonb_set(
                v_new_attr,
                '{ux,cardActivationBannerColors}',
                COALESCE(v_new_attr #> '{ux,cardActivationBannerColors}', '{}'::jsonb)
                || jsonb_build_object(
                    'bannerTextColor', '#0B0C0E',
                    'bannerBackgroundColor', '#E3E5E8',
                    'bannerTextColorOnActivation', '#FFFFFF',
                    'bannerBackgroundColorOnActivation', '#148D79'
                ),
                true
            );

            -- ux.button
            v_new_attr := jsonb_set(
                v_new_attr,
                '{ux,button}',
                COALESCE(v_new_attr #> '{ux,button}', '{}'::jsonb)
                || jsonb_build_object(
                    'primaryBgColor', '#326F91',
                    'primaryTextColor', '#FFFFFF'
                ),
                true
            );
			
			-- ux.disableButton
            v_new_attr := jsonb_set(
                v_new_attr,
                '{ux,disableButton}',
                COALESCE(v_new_attr #> '{ux,disableButton}', '{}'::jsonb)
                || jsonb_build_object(
                    'primaryTextColor', '#5F6062'
                ),
                true
            );
			
			-- ux.shopColors
            v_new_attr := jsonb_set(
                v_new_attr,
                '{ux,shopColors}',
                COALESCE(v_new_attr #> '{ux,shopColors}', '{}'::jsonb)
                || jsonb_build_object(
                    'storeCardNameLabelColor', '#0B0C0E',
					'purseDescriptionColor', '#3D3F42'
                ),
                true
            );

            -- includeHeaderFooter
            v_new_attr := jsonb_set(
                v_new_attr,
                '{includeHeaderFooter}',
                to_jsonb(true),
                true
            );

            -- displayAgreementsDeclineButton
            v_new_attr := jsonb_set(
                v_new_attr,
                '{displayAgreementsDeclineButton}',
                to_jsonb(true),
                true
            );
			
			-- enablePurseExpansion
			v_new_attr := jsonb_set(
                v_new_attr,
                '{enablePurseExpansion}',
                to_jsonb(true),
                true
            );

            -- headerColors
            v_new_attr := jsonb_set(
                v_new_attr,
                '{headerColors}',
                COALESCE(v_new_attr #> '{headerColors}', '{}'::jsonb)
                || jsonb_build_object('headerBgColor', '#FFFFFF'),
                true
            );

            -- ux.footerColors
            v_new_attr := jsonb_set(
                v_new_attr,
                '{ux,footerColors}',
                COALESCE(v_new_attr #> '{ux,footerColors}', '{}'::jsonb)
                || jsonb_build_object(
                    'footerBgColor', '#5F6062',
                    'footerTextColor', '#FFFFFF'
                ),
                true
            );

            -- ux.agreementColors
            v_new_attr := jsonb_set(
                v_new_attr,
                '{ux,agreementColors}',
                COALESCE(v_new_attr #> '{ux,agreementColors}', '{}'::jsonb)
                || jsonb_build_object(
                    'agreeCheckboxColor', '#E27025',
                    'declineButtonLabelColor', '#326F91'
                ),
                true
            );

            -- ux.forYouColors
            v_new_attr := jsonb_set(
                v_new_attr,
                '{ux,forYouColors}',
                COALESCE(v_new_attr #> '{ux,forYouColors}', '{}'::jsonb)
                || jsonb_build_object(
                    'forYouCardsButtonLabelColor', '#FFFFFF'
                ),
                true
            );

            -- headerImageUrls
            v_new_attr := jsonb_set(
                v_new_attr,
                '{headerImageUrls}',
                COALESCE(v_new_attr #> '{headerImageUrls}', '{}'::jsonb)
                || jsonb_build_object(
                    'headerMobileIconUrl',  v_mobileIconUrl,
                    'headerDesktopIconUrl', v_desktopIconUrl
                ),
                true
            );

            -- ux.tabBar
            v_new_attr := jsonb_set(
                v_new_attr,
                '{ux,tabBar}',
                COALESCE(v_new_attr #> '{ux,tabBar}', '{}'::jsonb)
                || jsonb_build_object('textColor', '#0B0C0E'),
                true
            );

            -- Save final JSON
            UPDATE tenant.tenant
            SET tenant_attr = v_new_attr,
                update_ts = NOW(),
                update_user = 'SYSTEM'
            WHERE tenant_code = v_tenant_code
              AND delete_nbr = 0;

            v_total_updated := v_total_updated + 1;
            RAISE NOTICE '✅ tenant_attr updated successfully for %', v_tenant_code;

        EXCEPTION WHEN OTHERS THEN
            v_total_errors := v_total_errors + 1;
            RAISE WARNING '❌ Failed to update tenant %: %', v_tenant_code, SQLERRM;
        END;
    END LOOP;

    -- Summary Output
    RAISE NOTICE '----------------------------------------';
    RAISE NOTICE 'Summary:';
    RAISE NOTICE 'Processed: %', v_total_processed;
    RAISE NOTICE 'Updated  : %', v_total_updated;
    RAISE NOTICE 'Errors   : %', v_total_errors;
    RAISE NOTICE '----------------------------------------';

END $$;
