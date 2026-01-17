-- =============================================================================
-- 🧩 Rollback Script
-- 📂 Schema    : tenant.tenant
-- 📌 Purpose   : Removes the 'richContentEnabled' flag from tenant_attr JSON 
--                for the specified tenant, effectively reverting the change.
-- 👨‍💻 Author    : Srikanth Kodam
-- 📅 Date       : 2025-10-23
-- 🧾 Jira       : BEN-182
-- ⚙️ Inputs :
--      v_tenant_codes            → Tenant identifier (e.g., '<KP-TENANT-CODE>')
-- 🔗 Script URL : <Optional Confluence or Documentation link>
-- 📝 Notes :
--   1. Checks if 'richContentEnabled' key exists in tenant_attr.
--   2. If exists, removes it and updates tenant_attr.
--   3. If not found, logs a notice — no update performed.
-- =============================================================================

DO $$
DECLARE
     v_tenant_codes TEXT[] := ARRAY[
       '<KP-TENANT-CODE>'
    ];
    v_tenant_code TEXT;
    v_old_attr    JSONB;                       
    v_new_attr    JSONB;                       
    v_updated     BOOLEAN := false;            
BEGIN

	FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
		-- Step-1 Fetch current tenant_attr JSON for the given tenant code
		SELECT tenant_attr
		INTO v_old_attr
		FROM tenant.tenant
		WHERE tenant_code = v_tenant_code
		  AND delete_nbr = 0
		  AND tenant_attr IS NOT NULL
		  AND tenant_attr::text <> '{}';

		IF NOT FOUND THEN
			RAISE WARNING 'No tenant found or tenant_attr is empty for tenant_code: %', v_tenant_code;
			RETURN;
		END IF;

		-- Step-2 Remove the 'richContentEnabled' flag if it exists
		IF (v_old_attr -> 'richContentEnabled') IS NOT NULL THEN
			v_new_attr := v_old_attr - 'richContentEnabled';
			v_updated := true;
			RAISE NOTICE '✅ richContentEnabled flag removed for tenant %', v_tenant_code;
		ELSE
			v_new_attr := v_old_attr;
			RAISE NOTICE 'richContentEnabled flag not found for tenant %, nothing to remove', v_tenant_code;
		END IF;

		-- Step-3 Update only if modification occurred
		IF v_updated THEN
			UPDATE tenant.tenant
			SET tenant_attr = v_new_attr
			WHERE tenant_code = v_tenant_code
			  AND delete_nbr = 0;

			RAISE NOTICE '✅ tenant_attr rollback completed successfully for tenant %', v_tenant_code;
		ELSE
			RAISE WARNING 'Rollback skipped — no richContentEnabled flag present for tenant %', v_tenant_code;
		END IF;
    END LOOP; -- tenant
END $$;
