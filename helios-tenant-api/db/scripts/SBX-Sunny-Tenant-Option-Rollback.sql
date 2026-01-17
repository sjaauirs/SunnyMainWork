-- 🔄 Script    : Rollback hamburgerMenu update under benefitsOptions for multiple tenants
-- 📌 Purpose   : Reverts "manageCard" back to "help" in benefitsOptions.hamburgerMenu
-- 🧑 Author    : Riaz Ahmed
-- 📅 Date      : 2025-11-12
-- 🧾 Jira      : 
-- ⚠️ Inputs    : v_tenant_codes (TEXT[])
-- 📤 Output    : tenant_option_json.benefitsOptions.hamburgerMenu updated for specified tenants
-- 📝 Notes     : Safe to rerun; avoids duplicates and preserves other entries

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'SUNNY-TENANT-CODE'
    ];
    v_tenant_code TEXT;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        -- 🔁 Step 1: Replace "manageCard" with "help"
        UPDATE tenant.tenant
        SET tenant_option_json = jsonb_set(
            tenant_option_json,
            '{benefitsOptions,hamburgerMenu}',
            to_jsonb(
                ARRAY(
                    SELECT DISTINCT
                        CASE
                            WHEN elem = 'manageCard' THEN 'help'
                            ELSE elem
                        END
                    FROM jsonb_array_elements_text(
                        COALESCE(tenant_option_json -> 'benefitsOptions' -> 'hamburgerMenu', '[]'::jsonb)
                    ) elem
                )
            ),
            true
        )
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        -- 🔁 Step 2: Add "help" if neither "help" nor "manageCard" existed before
        UPDATE tenant.tenant
        SET tenant_option_json = jsonb_set(
            tenant_option_json,
            '{benefitsOptions,hamburgerMenu}',
            to_jsonb(
                ARRAY(
                    SELECT DISTINCT elem
                    FROM (
                        SELECT elem
                        FROM jsonb_array_elements_text(
                            COALESCE(tenant_option_json -> 'benefitsOptions' -> 'hamburgerMenu', '[]'::jsonb)
                        ) elem
                        UNION ALL
                        SELECT 'help'
                    ) AS all_items
                )
            ),
            true
        )
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
          AND NOT EXISTS (
              SELECT 1
              FROM jsonb_array_elements_text(
                  COALESCE(tenant_option_json -> 'benefitsOptions' -> 'hamburgerMenu', '[]'::jsonb)
              ) elem
              WHERE elem IN ('help', 'manageCard')
          );

        RAISE NOTICE '🔁 Replaced "manageCard" with "help" (or added if missing) for tenant %', v_tenant_code;
    END LOOP;
END $$;