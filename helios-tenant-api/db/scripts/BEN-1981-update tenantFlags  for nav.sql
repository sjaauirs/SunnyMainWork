-- ============================================================================
-- 🚀 Script    : update_shouldFreezeCardOnTermination_multi_tenant.sql
-- 🎯 Purpose   : Sets shouldFreezeCardOnTermination=true and validCardActiveDays=30
-- 🧾 Jira      : BEN-1981
-- 📝 Summary   :
--               • Iterates through tenant list
--               • Ensures benefitsOptions exists in tenant_option_json
--               • Updates:
--                   - benefitsOptions.shouldFreezeCardOnTermination = true
--                   - benefitsOptions.validCardActiveDays = 30
--               • Safe to rerun (idempotent)
--
-- 📌 Parameters:
--      ▪ v_tenant_codes   : Array of tenant codes
--
-- ============================================================================
DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        '<NAV-TENANT-CODE>',
        '<NAV-TENANT-CODE>'
    ];
    v_code TEXT;
    v_json JSONB;
BEGIN
    FOREACH v_code IN ARRAY v_tenant_codes
    LOOP
        SELECT tenant_option_json INTO v_json
        FROM tenant.tenant
        WHERE tenant_code = v_code;

        IF v_json IS NULL THEN
            RAISE NOTICE '⚠️ Skipped: No tenant_option_json found for %', v_code;
            CONTINUE;
        END IF;

        IF v_json ? 'benefitsOptions' THEN
            v_json :=
                jsonb_set(
                    jsonb_set(
                        v_json,
                        '{benefitsOptions,shouldFreezeCardOnTermination}',
                        'true'::jsonb,
                        true
                    ),
                    '{benefitsOptions,validCardActiveDays}',
                    '30'::jsonb,
                    true
                );

            UPDATE tenant.tenant
            SET tenant_option_json = v_json
            WHERE tenant_code = v_code;

            RAISE NOTICE '✅ Updated freeze + activeDays for %', v_code;
        ELSE
            RAISE NOTICE '⚠️ Skipped: benefitsOptions not available for %', v_code;
        END IF;
    END LOOP;
END $$;
