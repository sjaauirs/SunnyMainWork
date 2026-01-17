-- ============================================================================
-- 🚀 Script    : Rollback IncludeDiscretionaryCardData flag in tenant_option_json
-- 📌 Purpose   : Resets tenant_option_json->benefitsOptions->IncludeDiscretionaryCardData
--                to FALSE for a list of tenant_codes
-- 🧑 Author    : Saurabh
-- 📅 Date      : 2025-09-24
-- 🧾 Jira      : BEN-654
-- ⚠️ Inputs    : v_tenant_codes (Array of tenant identifiers)
-- 📤 Output    : Updates tenant_option_json JSONB by setting the key to FALSE
-- 🔗 Script URL: https://github.com/SunnyRewards/helios-tenant-api/blob/develop/db/scripts/BEN-572_set_include_discretionary_card_data_false.sql
-- 📝 Notes     : Runs idempotently; will create the key if missing, and safely skips
--                tenants that are deleted or not found
-- ============================================================================

DO $$
DECLARE
    -- List of tenants to process
    v_tenant_codes TEXT[] := ARRAY[
        '<HAP-TENANT-CODE-1>',
        '<HAP-TENANT-CODE-2>',
        '<HAP-TENANT-CODE-3>'
    ];
    v_tenant_code TEXT;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        BEGIN
            -- Rollback: set IncludeDiscretionaryCardData = false
            UPDATE tenant.tenant
            SET tenant_option_json = jsonb_set(
                tenant_option_json,
                '{benefitsOptions,IncludeDiscretionaryCardData}',
                'false'::jsonb,
                true
            )
            WHERE tenant_code = v_tenant_code
              AND delete_nbr = 0;

            IF NOT FOUND THEN
                RAISE NOTICE 'No matching tenant rolled back for tenant_code: %', v_tenant_code;
            ELSE
                RAISE NOTICE 'Successfully rolled back tenant_code: %', v_tenant_code;
            END IF;

        EXCEPTION
            WHEN OTHERS THEN
                RAISE WARNING 'Error rolling back tenant_code %: %',
                    v_tenant_code, SQLERRM;
        END;
    END LOOP;
END $$;
