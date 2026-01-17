
-- ============================================================================
-- 🚀 Script: Rollback tenant_option_json in tenant.tenant table
-- 📌 Purpose: Rollback config for skip transaction
-- 🧑 Author  : Kawalpreet kaur
-- 📅 Date    : 2025-12-03
-- 🧾 Jira    : RES-1236
-- ⚠️  Inputs: KP tenant code
-- ============================================================================DO $$
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
        
        RAISE NOTICE '🔍 Processing tenant code for rollback: %', v_code;

        -- Check if tenant exists
        SELECT TRUE
        INTO v_exists
        FROM tenant.tenant
        WHERE tenant_code = v_code
        LIMIT 1;

        IF NOT v_exists THEN
            RAISE NOTICE '❌ Tenant % does NOT exist. Skipping rollback...', v_code;
            CONTINUE;
        END IF;

        RAISE NOTICE '✔ Tenant % exists. Removing skipTransactionTypes key...', v_code;

        BEGIN
            UPDATE tenant.tenant
            SET tenant_option_json =
                COALESCE(tenant_option_json, '{}'::jsonb) - 'skipTransactionTypes'
            WHERE tenant_code = v_code;

            RAISE NOTICE '🔄 Rollback complete for tenant % (skipTransactionTypes removed)', v_code;

        EXCEPTION WHEN OTHERS THEN
            RAISE NOTICE '❌ Rollback failed for %: %', v_code, SQLERRM;
        END;

    END LOOP;

END $$;