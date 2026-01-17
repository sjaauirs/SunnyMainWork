-- =============================================================================
-- 🧩 Rollback Script
-- 📂 Schema    : tenant.tenant
-- 📌 Purpose   : Removes the 'richContentEnabled' flag from tenant_attr JSON 
--                for the specified tenant(s), effectively reverting the change.
-- 👨‍💻 Author    : Srikanth Kodam
-- 📅 Date       : 2025-11-12
-- 🧾 Jira       : BEN-1964
-- ⚙️ Inputs :
--      v_tenant_codes → Array of tenant identifiers (e.g., ['<HAP-TENANT-CODE>'])
-- 🔗 Script URL : <Optional Confluence or Documentation link>
-- 📝 Notes :
--   1. Checks if 'richContentEnabled' key exists in tenant_attr.
--   2. If exists, removes it and updates tenant_attr.
--   3. If not found, logs a notice — no update performed.
--   4. Safe and idempotent — can be re-run without side effects.
-- =============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        '<HAP-TENANT-CODE>'  -- Replace or add multiple tenant codes as needed
    ];
    v_tenant_code TEXT;  
    v_old_attr    JSONB;                       
    v_new_attr    JSONB;                       
    v_updated     BOOLEAN;            
BEGIN
    -- TENANT LOOP
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
          AND tenant_attr::TEXT <> '{}';

        IF NOT FOUND THEN
            RAISE WARNING '⚠️ No tenant found or tenant_attr is empty for tenant_code: %', v_tenant_code;
            CONTINUE;
        END IF;

        -- Step 2️: Remove the 'richContentEnabled' flag (if exists)
        
        IF (v_old_attr ? 'richContentEnabled') THEN
            v_new_attr := v_old_attr - 'richContentEnabled';
            v_updated := TRUE;
            RAISE NOTICE '🗑️ "richContentEnabled" flag removed for tenant %', v_tenant_code;
        ELSE
            v_new_attr := v_old_attr;
            RAISE NOTICE 'ℹ️ "richContentEnabled" flag not found for tenant %, nothing to remove', v_tenant_code;
        END IF;

        -- Step 3️: Update tenant_attr only if modified
        
        IF v_updated THEN
            UPDATE tenant.tenant
            SET tenant_attr = v_new_attr,
                update_ts   = NOW(),
                update_user = 'SYSTEM'
            WHERE tenant_code = v_tenant_code
              AND delete_nbr = 0;

            RAISE NOTICE '✅ tenant_attr rollback completed successfully for tenant %', v_tenant_code;
        ELSE
            RAISE NOTICE ' Rollback skipped — no "richContentEnabled" flag present for tenant %', v_tenant_code;
        END IF;

    END LOOP;

    RAISE NOTICE '🎯 Rollback completed for all tenants in the provided list.';
END $$;
