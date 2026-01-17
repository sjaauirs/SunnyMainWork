
-- ============================================================================
-- 🚀 Script: Update tenant_option_json in tenant.tenant table
-- 📌 Purpose: Add config for skip transaction
-- 🧑 Author  : Kawalpreet kaur
-- 📅 Date    : 2025-12-03
-- 🧾 Jira    : RES-1236
-- ⚠️  Inputs: HAP tenant code
-- ============================================================================
DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        '<HAP_Tenant_Code_1>',
        '<HAP_Tenant_Code_2>',
        '<HAP_Tenant_Code_3>'
    ];

    v_code TEXT;
    v_exists BOOLEAN;
BEGIN
    FOREACH v_code IN ARRAY v_tenant_codes LOOP
        
        RAISE NOTICE '🔍 Processing tenant code: %', v_code;

        -- Check if tenant exists
        SELECT TRUE
        INTO v_exists
        FROM tenant.tenant
        WHERE tenant_code = v_code
        LIMIT 1;

        IF NOT v_exists THEN
            RAISE NOTICE '❌ Tenant % does NOT exist. Skipping...', v_code;
            CONTINUE;
        END IF;

        RAISE NOTICE '✔ Tenant % exists. Updating tenant_option JSON...', v_code;

        BEGIN
            UPDATE tenant.tenant
            SET tenant_option_json =
                COALESCE(tenant_option_json, '{}'::jsonb) ||
                jsonb_build_object(
                    'skipTransactionTypes',
                    jsonb_build_array('CARD')
                )
            WHERE tenant_code = v_code;

            RAISE NOTICE '✅ Successfully updated tenant %', v_code;

        EXCEPTION WHEN OTHERS THEN
            RAISE NOTICE '❌ Update failed for %: %', v_code, SQLERRM;
        END;

    END LOOP;

END $$;