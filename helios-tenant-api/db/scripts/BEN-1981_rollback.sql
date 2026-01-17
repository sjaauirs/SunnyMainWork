-- ============================================================================
-- 🚨 Rollback Script : update_shouldFreezeCardOnTermination_multi_tenant_rollback.sql
-- 🎯 Purpose         : Reverts shouldFreezeCardOnTermination + validCardActiveDays changes
-- 🧾 Jira            : BEN-1981
-- 📝 Summary         :
--                     • Iterates through tenant list
--                     • Ensures benefitsOptions exists in tenant_option_json
--                     • Reverts:
--                         - shouldFreezeCardOnTermination → false
--                         - validCardActiveDays → removed (set to NULL)
--                     • Safe to rerun (idempotent)
--
-- 📌 Parameters:
--      ▪ v_tenant_codes   : Array of tenant codes
--
-- ============================================================================
DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAV-TENANT-CODE',
        'NAV-TENANT-CODE'
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
            RAISE NOTICE '⚠️ Rollback skipped: No tenant_option_json found for %', v_code;
            CONTINUE;
        END IF;

        IF v_json ? 'benefitsOptions' THEN

            -- Revert shouldFreezeCardOnTermination = false
            v_json := jsonb_set(
                v_json,
                '{benefitsOptions,shouldFreezeCardOnTermination}',
                'false'::jsonb,
                true
            );

            -- Remove validCardActiveDays (set null removes key)
            v_json := jsonb_set(
                v_json,
                '{benefitsOptions,validCardActiveDays}',
                'null'::jsonb,
                true
            );

            UPDATE tenant.tenant
            SET tenant_option_json = v_json
            WHERE tenant_code = v_code;

            RAISE NOTICE '🔄 Rolled back freeze + activeDays for %', v_code;
        ELSE
            RAISE NOTICE '⚠️ Rollback skipped: benefitsOptions not available for %', v_code;
        END IF;
    END LOOP;
END $$;
