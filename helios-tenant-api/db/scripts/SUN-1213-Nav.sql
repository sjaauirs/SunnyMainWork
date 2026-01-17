-- ============================================================================
-- 🚀 Script    : Update UX Colors & Flags in tenant_attr
-- 📌 Purpose   : Updates multiple UX color fields & boolean flags for given tenants
-- 👨‍💻 Author    : Riaz
-- 📅 Date      : 2025-12-01
-- 🧾 Jira      : SUN-1213
-- ⚠️ Inputs    : v_tenant_codes[]
-- 📤 Output    : Updates tenant_attr JSON structure
-- 📝 Notes     :
--    - Idempotent: safe to run multiple times.
--    - Initializes missing JSON objects automatically.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant TEXT;
BEGIN
    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP

        UPDATE tenant.tenant t
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
                                    COALESCE(t.tenant_attr::jsonb, '{}'::jsonb),

                                    '{ux,commonColors,screenBgColor}',
                                    '"#F7F7F7"'::jsonb,
                                    true
                                  ),
                                  '{ux,commonColors,disableToggleBgColor}',
                                  '"#5F6062"'::jsonb,
                                  true
                                ),
                                '{ux,commonColors,borderColor2}',
                                '"#CBCCCD"'::jsonb,
                                true
                              ),
                              '{ux,commonColors,borderColor}',
                              '"#E27025"'::jsonb,
                              true
                            ),
                            '{ux,commonColors,menuIconColor}',
                            '"#0B0C0E"'::jsonb,
                            true
                          ),
                          '{ux,commonColors,textColor}',
                          '"#0B0C0E"'::jsonb,
                          true
                        ),
                        '{ux,commonColors,dollar}',
                        '"#0B0C0E"'::jsonb,
                        true
                      ),
                      '{ux,themeColors,accent3}',
                      '"#FFFFFF"'::jsonb,
                      true
                    ),
                    '{hideRedeemHeader}',
                    'true'::jsonb,
                    true
                  ),
                  '{hideRewardsGiftBoxIcon}',
                  'true'::jsonb,
                  true
                ),
                '{ux,mycardColors,walletBgColor}',
                '"#F7F4F0"'::jsonb,
                true
              ),
              '{updatedByScript}',
              '"UX_COLOR_UPDATE_2025_12_01"'::jsonb,
              true
            ),
            update_ts = NOW()
        WHERE t.tenant_code = v_tenant
          AND t.delete_nbr = 0;

        RAISE NOTICE '✅ Updated UX colors & flags for tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🏁 Completed UX updates for all provided tenants.';
END $$;


-- ============================================================================
-- 🚀 Script    : Update UX colors in tenant_attr
-- 📌 Purpose   : Updates various UX color fields under commonColors & themeColors
-- 👨‍💻 Author    : Riaz
-- 📅 Date      : 2025-12-05
-- 🧾 Jira      : SUN-1213
-- ⚠️ Inputs    : v_tenant_codes[]
-- 📤 Output    : Updates tenant_attr JSON structure
-- 📝 Notes     :
--    - Idempotent: safe to run multiple times.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant TEXT;
BEGIN
    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP

        UPDATE tenant.tenant t
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
                              COALESCE(t.tenant_attr::jsonb, '{}'::jsonb),

                              '{ux,themeColors,headerBgColor}',
                              '"#0B0C0E"'::jsonb,
                              true
                            ),
                            '{ux,commonColors,contentBgColor}',
                            '"#FFFFFF"'::jsonb,
                            true
                          ),
                          '{ux,commonColors,buttonTextColor2}',
                          '"#0B0C0E"'::jsonb,
                          true
                        ),
                        '{ux,commonColors,textColor}',
                        '"#0B0C0E"'::jsonb,
                        true
                      ),
                      '{ux,commonColors,buttonColor}',
                      '"#E27025"'::jsonb,
                      true
                    ),
                    '{ux,themeColors,RewardDialTextColor}',
                    '"#0B0C0E"'::jsonb,
                    true
                  ),
                  '{ux,commonColors,paginationDotActiveColor}',
                  '"#326F91"'::jsonb,
                  true
                ),
                '{ux,commonColors,forYouCardsBgColor}',
                '"#FFFFFF"'::jsonb,
                true
              ),
              '{updatedByScript}',
              '"UX_COLOR_UPDATE_2025_12_05"'::jsonb,
              true
            ),
            update_ts = NOW()
        WHERE t.tenant_code = v_tenant
          AND t.delete_nbr = 0;

        RAISE NOTICE '✅ Updated UX colors for tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🏁 Completed UX updates for all provided tenants.';
END $$;


-- ============================================================================
-- 🚀 Script    : Update Header & Theme Colors in tenant_attr
-- 📌 Purpose   : Updates:
--                  - ux.headerColors.headerBgColor
--                  - ux.themeColors.accent1
--                  - ux.themeColors.CalendarSelectedDayColor
-- 👨‍💻 Author    : Riaz
-- 📅 Date      : 2025-12-08
-- 🧾 Jira      : SUN-1213
-- ⚠️ Inputs    : v_tenant_codes[]
-- 📤 Output    : Updated tenant_attr JSON structure
-- 📝 Notes     :
--    - Idempotent: safe to run multiple times.
--    - Initializes missing JSON objects automatically.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant TEXT;
BEGIN
    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP

        UPDATE tenant.tenant t
        SET tenant_attr =
            jsonb_set(
                jsonb_set(
                    jsonb_set(
                        COALESCE(t.tenant_attr::jsonb, '{}'::jsonb),

                        -- 1️⃣ headerColors.headerBgColor = "#FFFFFF"
                        '{ux,headerColors,headerBgColor}',
                        '"#FFFFFF"'::jsonb,
                        true
                    ),

                    -- 2️⃣ themeColors.accent1 = "#326F91"
                    '{ux,themeColors,accent1}',
                    '"#326F91"'::jsonb,
                    true
                ),

                -- 3️⃣ themeColors.CalendarSelectedDayColor = "#326F91"
                '{ux,themeColors,CalendarSelectedDayColor}',
                '"#326F91"'::jsonb,
                true
            )
            || jsonb_build_object('updatedByScript', 'UX_HEADER_THEME_UPDATE_2025_12_08'),
            update_ts = NOW()
        WHERE t.tenant_code = v_tenant
          AND t.delete_nbr = 0;

        RAISE NOTICE '✅ Updated headerColors & themeColors for tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🏁 Completed updates for all tenants.';
END $$;

-- ============================================================================
-- 🚀 Script    : Update Card Activation Banner & Shop Colors in tenant_attr
-- 📌 Purpose   : Updates:
--                  - ux.cardActivationBannerColors.bannerBackgroundColorOnActivation
--                  - ux.cardActivationBannerColors.bannerTextColorOnActivation
--                  - ux.shopColors.storeCardNameLabelColor
-- 👨‍💻 Author    : Riaz
-- 📅 Date      : 2025-12-12
-- 🧾 Jira      : SUN-1213
-- ⚠️ Inputs    : v_tenant_codes[]
-- 📤 Output    : Updated tenant_attr JSON structure
-- 📝 Notes     :
--    - Idempotent: safe to run multiple times.
--    - Initializes missing JSON objects automatically.
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant TEXT;
BEGIN
    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP

        UPDATE tenant.tenant t
        SET tenant_attr = (
            jsonb_set(
                jsonb_set(
                    jsonb_set(
                        jsonb_set(
                            COALESCE(t.tenant_attr::jsonb, '{}'::jsonb),
                            '{ux,cardActivationBannerColors,bannerBackgroundColorOnActivation}',
                            '"#E3E5E8"'::jsonb,
                            true
                        ),
                        '{ux,cardActivationBannerColors,bannerTextColorOnActivation}',
                        '"#0B0C0E"'::jsonb,
                        true
                    ),
                    '{ux,shopColors,storeCardNameLabelColor}',
                    '"#0B0C0E"'::jsonb,
                    true
                ),
                '{}', 
                '{}'::jsonb,
                true
            )
        ) || jsonb_build_object('updatedByScript', 'UX_CARD_BANNER_SHOPCOLOR_UPDATE_2025_12_12'),
        update_ts = NOW()
        WHERE t.tenant_code = v_tenant
          AND t.delete_nbr = 0;

        RAISE NOTICE '✅ Updated cardActivationBannerColors & shopColors for tenant: %', v_tenant;
    END LOOP;

    RAISE NOTICE '🏁 Completed updates for all tenants.';
END $$;
