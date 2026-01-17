-- ============================================================================
-- 🚀 Script    : Revert tenant_attr properties for Watco
-- 📌 Purpose   : Revert tenant_attr properties for Watco
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
       'WATCO-TENANT-CODE'
    ];
    v_tenant TEXT;
BEGIN
    FOREACH v_tenant IN ARRAY v_tenant_codes LOOP
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
                jsonb_set(
                jsonb_set(
                    jsonb_set(
                    jsonb_set(
                        jsonb_set(
                        jsonb_set(
                            jsonb_set(
                            tenant_attr,
                            -- 1. Remove ux.triviaColors.nextButtonBgColor
                            '{ux,triviaColors,nextButtonBgColor}',
                            'null'::jsonb,
                            false
                            ),
                            -- 2. Remove ux.triviaColors.nextButtonBorderColor
                            '{ux,triviaColors,nextButtonBorderColor}',
                            'null'::jsonb,
                            false
                        ),
                        -- 3. Remove ux.triviaColors.nextButtonTextColor
                        '{ux,triviaColors,nextButtonTextColor}',
                        'null'::jsonb,
                        false
                        ),
                        -- 4. Remove ux.button.primaryTextColor (was not present originally)
                        '{ux,button,primaryTextColor}',
                        'null'::jsonb,
                        false
                    ),
                    -- 5. Remove ux.taskTileColors.checkBoxBgColor (restore original #?)
                    '{ux,taskTileColors,checkBoxBgColor}',
                    to_jsonb('#1B416D'::text),  -- ORIGINAL DEFAULT VALUE
                    true
                    ),
                    -- 6. Remove ux.button.primaryBgColor
                    '{ux,button,primaryBgColor}',
                    'null'::jsonb,
                    false
                ),
                -- 7. Restore original themeColors.accent1 value
                '{ux,themeColors,accent1}',
                to_jsonb('#0F4F8B'::text),
                true
                ),
                -- 8. Restore original themeColors.accent3 value
                '{ux,themeColors,accent3}',
                to_jsonb('#FFFFFF'::text),
                true
            ),
            update_ts = NOW()
        WHERE tenant_code = v_tenant
        AND delete_nbr = 0;

        RAISE NOTICE '♻️ Rollback completed for tenant: %', v_tenant;
    END LOOP;
END $$;
