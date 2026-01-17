-- =============================================================================
-- 🚀 Script    : Add 'richContentEnabled' flag to tenant_attr JSON for given tenants
-- 📂 Schema    : tenant.tenant
-- 📌 Purpose   : 
--   This script checks the 'tenant_attr' JSON column for the specified tenants.
--   If the key 'richContentEnabled' does not exist, it adds it with value `true`.
--   Updates are performed only when the flag is missing to avoid redundant writes.
-- 👨‍💻 Author    : Srikanth Kodam
-- 📅 Date       : 2025-11-12
-- 🧾 Jira       : BEN-1964
-- ⚙️ Inputs :
--      v_tenant_codes  → List of tenant identifiers (e.g., 'ten-xxxxx')
-- 🔗 Script URL : <Optional Confluence or Documentation link>
-- 📝 Notes :
--   1. Retrieves the tenant_attr JSON for each tenant.
--   2. Checks if 'richContentEnabled' key exists.
--   3. Adds the key if missing and updates the DB record.
--   4. Skips update if key already exists.
-- =============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        '<HAP-TENANT-CODE>'  -- Replace or add multiple tenant codes as needed
    ];
    v_tenant_code TEXT;
    v_old_attr JSONB;                          
    v_new_attr JSONB;                          
    v_updated BOOLEAN;                
BEGIN

    -- 🔁 TENANT LOOP
    FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        v_updated := FALSE;

        RAISE NOTICE '🚀 Processing tenant: %', v_tenant_code;

        -- Step 1️: Fetch current tenant_attr JSON
        SELECT tenant_attr
        INTO v_old_attr
        FROM tenant.tenant
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
          AND tenant_attr IS NOT NULL
          AND tenant_attr::text <> '{}';

        IF NOT FOUND THEN
            RAISE WARNING '⚠️ No tenant found or tenant_attr is empty for tenant_code: %', v_tenant_code;
            CONTINUE;
        END IF;

        v_new_attr := v_old_attr;

        -- Step 2️: Add 'richContentEnabled' flag if missing
        IF (v_new_attr -> 'richContentEnabled') IS NULL THEN
            v_new_attr := jsonb_set(v_new_attr, '{richContentEnabled}', 'true'::jsonb, true);
            v_updated := TRUE;
            RAISE NOTICE '✅ Added richContentEnabled flag for tenant: %', v_tenant_code;
        ELSE
            RAISE NOTICE 'ℹ️ richContentEnabled flag already exists for tenant: %', v_tenant_code;
        END IF;

        -- Step 3️: Update tenant_attr only if modified
        IF v_updated THEN
            UPDATE tenant.tenant
            SET tenant_attr = v_new_attr
            WHERE tenant_code = v_tenant_code
              AND delete_nbr = 0;

            RAISE NOTICE '🎯 tenant_attr updated successfully for tenant: %', v_tenant_code;
        ELSE
            RAISE NOTICE ' No update required for tenant: %', v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE '✅ Processing completed for all tenants.';
END $$;
