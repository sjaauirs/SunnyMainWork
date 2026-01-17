
-- ============================================================================
-- 🚀 Script    : add_menuNavigation_to_benefitOptions_array
-- 📌 Purpose   : Add "menuNavigation" flag under "benefitsOptions" for selected tenants
-- 🧑 Author    : Saurabh Jaiswal
-- 📅 Date      : 2025-11-13
-- 🧾 Jira      : BEN-1986
-- ⚙️ Notes     :
--                 • Processes an explicit list of tenant_codes.
--                 • Adds "menuNavigation": "Bottom" only if missing.
--                 • Safe to rerun (idempotent).
-- ============================================================================

DO
$$
DECLARE
    -- 🔹 Provide all NAV tenant codes here
    v_tenant_codes TEXT[] := ARRAY[
        '<NAV-TENANT-CODE>'
        -- Add more as needed
    ];

    v_tenant_code TEXT;
    v_rowcount INT;
BEGIN
    RAISE NOTICE '🚀 Starting menuNavigation update for NAV tenants...';

    FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        RAISE NOTICE '➡️ Processing tenant: %', v_tenant_code;

        UPDATE tenant.tenant
        SET tenant_option_json = jsonb_set(
                tenant_option_json,
                '{benefitsOptions,menuNavigation}',
                '"Bottom"'::jsonb,
                true
            ),
            update_ts   = NOW(),
            update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND tenant_option_json IS NOT NULL
          AND tenant_option_json <> '{}'::jsonb
          AND tenant_option_json ? 'benefitsOptions'
          AND NOT (tenant_option_json->'benefitsOptions') ? 'menuNavigation'
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;

        IF v_rowcount > 0 THEN
            RAISE NOTICE '✅ Updated tenant: % (menuNavigation added)', v_tenant_code;
        ELSE
            RAISE NOTICE 'ℹ️ No change needed for tenant: % (already had menuNavigation or no benefitsOptions)', v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE '🏁 Completed menuNavigation updates for all listed tenants.';
END
$$;


-- ============================================================================
-- 🚀 Script    : safe_add_benefitsOptions_menus_array.sql
-- 📌 Purpose   : Safely add or update "primaryMenu", "hamburgerMenu",
--                and "secondaryMenu" under "benefitsOptions" for selected tenants
-- 🧑 Author    : Saurabh Jaiswal
-- 📅 Date      : 2025-11-13
-- 🧾 Jira      : BEN-1986
-- ⚙️ Notes     :
--                 • Does NOT remove existing keys (merges instead).
--                 • Adds menu arrays only if missing or outdated.
--                 • Safe to rerun (idempotent).
-- ============================================================================

DO
$$
DECLARE
    -- 🔹 NAV tenant codes to process
    v_tenant_codes TEXT[] := ARRAY[
        '<NAV-TENANT-CODE>'
        -- Add more tenant codes as needed
    ];

    v_tenant_code TEXT;
    v_rowcount INT;
BEGIN
    RAISE NOTICE '🚀 Starting SAFE benefitsOptions menu setup for NAV tenants...';

    FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        RAISE NOTICE '➡️ Processing tenant: %', v_tenant_code;

        UPDATE tenant.tenant
        SET tenant_option_json = 
            jsonb_set(
                -- ensure benefitsOptions exists
                COALESCE(tenant_option_json, '{}'::jsonb)
                  || jsonb_build_object(
                       'benefitsOptions',
                       COALESCE(tenant_option_json->'benefitsOptions','{}'::jsonb)
                     ),
                '{benefitsOptions}',
                (
                    -- merge existing with new keys instead of overwriting
                    COALESCE(tenant_option_json->'benefitsOptions', '{}'::jsonb)
                    || jsonb_build_object(
                        'primaryMenu',   '["myCard","myRewards","shop"]'::jsonb,
                        'hamburgerMenu', '["personal","notifications","manageCard","help"]'::jsonb,
                        'secondaryMenu', '["agreements","privacyPolicy","signOut"]'::jsonb
                    )
                ),
                true
            ),
            update_ts   = NOW(),
            update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;

        IF v_rowcount > 0 THEN
            RAISE NOTICE '✅ Updated benefitsOptions menus for tenant: %', v_tenant_code;
        ELSE
            RAISE NOTICE 'ℹ️ No rows affected (tenant may not exist or already updated): %', v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE '🏁 Completed SAFE benefitsOptions menu setup for all tenants.';
END
$$;




-- ============================================================================
-- 🚀 Script    : add_hamburgerMenuIcons_multi_tenant.sql
-- 📌 Purpose   : Add or update "hamburgerMenuIcons" under "benefitsOptions"
--                for multiple NAV tenants, with environment-based icon URLs.
-- 🧑 Author    : Saurabh Jaiswal
-- 📅 Date      : 2025-11-13
-- 🧾 Jira      : BEN-1986
-- ⚙️ Notes     :
--                 • Safe to rerun (idempotent).
--                 • Set the environment once; applies to all tenants in the array.
-- ============================================================================
DO
$$
DECLARE
    -- 🔹 List of tenant codes
    v_tenant_codes TEXT[] := ARRAY[
        '<NAV-TENANT_CODE>'
        -- Add more tenant codes as needed
    ];

    -- 🔹 Environment (DEV / QA / UAT / INTEG / PROD)
    v_env TEXT := '<ENV>';

    v_tenant_code TEXT;
    v_env_base_url TEXT;
    v_icons_json JSONB;
BEGIN
    -- ------------------------------------------------------------
    -- 🌍 Resolve environment-specific base URL (tenant-specific)
    -- ------------------------------------------------------------
    CASE v_env
        WHEN 'DEV'   THEN v_env_base_url := 'https://app-static.dev.sunnyrewards.com/cms/icons';
        WHEN 'QA'    THEN v_env_base_url := 'https://app-static.qa.sunnyrewards.com/cms/icons';
        WHEN 'UAT'   THEN v_env_base_url := 'https://app-static.uat.sunnyrewards.com/cms/icons';
        WHEN 'INTEG' THEN v_env_base_url := 'https://app-static.integ.sunnyrewards.com/cms/icons';
        WHEN 'PROD'  THEN v_env_base_url := 'https://app-static.sunnyrewards.com/cms/icons';
        ELSE
            RAISE EXCEPTION '❌ Invalid environment [%]. Choose DEV/QA/UAT/INTEG/PROD.', v_env;
    END CASE;


    -- ------------------------------------------------------------
    -- 🔁 Iterate through each tenant and build tenant-based JSON
    -- ------------------------------------------------------------
    FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        RAISE NOTICE '➡️ Processing tenant [%]', v_tenant_code;

        -- Build JSON with ENV + TENANT dynamic URL prefix
        v_icons_json := jsonb_build_object(
            'myCard',                   v_env_base_url || '/' || v_tenant_code || '/menu_nav_mycard_icon.png',
            'myRewards',                v_env_base_url || '/' || v_tenant_code || '/menu_nav_rewards_icon.png',
            'personal',                 v_env_base_url || '/' || v_tenant_code || '/menu_nav_personal_icon.png',
            'notifications',            v_env_base_url || '/' || v_tenant_code || '/menu_nav_notificatios_icon.png',
            'manageCard',               v_env_base_url || '/' || v_tenant_code || '/menu_nav_manage_card_icon.png',
            'help',                     v_env_base_url || '/' || v_tenant_code || '/menu_nav_help_icon.png',
            'shop',                     v_env_base_url || '/' || v_tenant_code || '/menu_nav_shop_icon.png',
            'help_selected',            v_env_base_url || '/' || v_tenant_code || '/menu_nav_help_icon_selected.png',
            'shop_selected',            v_env_base_url || '/' || v_tenant_code || '/menu_nav_shop_icon_selected.png',
            'myCard_selected',          v_env_base_url || '/' || v_tenant_code || '/menu_nav_mycard_icon_selected.png',
            'personal_selected',        v_env_base_url || '/' || v_tenant_code || '/menu_nav_personal_icon_selected.png',
            'myRewards_selected',       v_env_base_url || '/' || v_tenant_code || '/menu_nav_rewards_icon_selected.png',
            'manageCard_selected',      v_env_base_url || '/' || v_tenant_code || '/menu_nav_manage_card_icon_selected.png',
            'notifications_selected',   v_env_base_url || '/' || v_tenant_code || '/menu_nav_notificatios_icon_selected.png'
        );

        -- ------------------------------------------------------------
        -- Update tenant table
        -- ------------------------------------------------------------
        UPDATE tenant.tenant
        SET tenant_option_json = jsonb_set(
                COALESCE(tenant_option_json, '{}'::jsonb),
                '{benefitsOptions,hamburgerMenuIcons}',
                v_icons_json,
                true
            ),
            update_ts   = NOW(),
            update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '✅ hamburgerMenuIcons updated for tenant [%] in env [%]', v_tenant_code, v_env;
    END LOOP;

    RAISE NOTICE '🏁 Completed hamburgerMenuIcons updates for all tenants in env [%]', v_env;
END
$$;
