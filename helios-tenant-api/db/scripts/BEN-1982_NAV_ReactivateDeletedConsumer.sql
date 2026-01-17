-- ============================================================================
-- 🚀 Script    : update_reactivateDeletedConsumer_multi_tenant.sql
-- 🎯 Purpose   : Sets ReactivateDeletedConsumer = true for multiple tenants
-- 🧾 Jira      : BEN-1982
-- 📝 Summary   :
--               • Iterates through tenant list
--               • Ensures benefitsOptions exists in tenant_option_json
--               • Updates:
--                   - benefitsOptions.ReactivateDeletedConsumer = true
--               • Safe to rerun (idempotent)
--
-- 📌 Parameters:
--      ▪ v_tenant_codes   : Array of tenant codes
--
-- ============================================================================
DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        '<NAV_TENANT_CODE>',
        '<NAV_TENANT_CODE>'
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
                    v_json,
                    '{benefitsOptions,ReactivateDeletedConsumer}',
                    'true'::jsonb,
                    true
                );

            UPDATE tenant.tenant
            SET tenant_option_json = v_json
            WHERE tenant_code = v_code;

            RAISE NOTICE '✅ Updated ReactivateDeletedConsumer for %', v_code;
        ELSE
            RAISE NOTICE '⚠️ Skipped: benefitsOptions not available for %', v_code;
        END IF;
    END LOOP;
END $$;
