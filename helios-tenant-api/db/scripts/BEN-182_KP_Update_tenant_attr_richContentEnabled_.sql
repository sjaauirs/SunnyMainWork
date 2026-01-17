
-- =============================================================================
-- 🚀 Script    : Add 'richContentEnabled' flag to tenant_attr JSON for a given tenant
-- 📂 Schema    : tenant.tenant
-- 📌 Purpose   : 
--   This script checks the 'tenant_attr' JSON column for the specified tenant.
--   If the key 'richContentEnabled' does not exist, it adds it with value `true`.
--   Updates are made only if the flag is missing to avoid redundant writes.
-- 👨‍💻 Author    : Srikanth Kodam
-- 📅 Date       : 2025-10-23
-- 🧾 Jira       : BEN-182
-- ⚙️ Inputs :
--      v_tenant_codes            → Tenant identifier (e.g., '<KP-TENANT-CODE>')
-- 🔗 Script URL : <Optional Confluence or Documentation link>
-- 📝 Notes :
--   1. Retrieve the tenant_attr JSON for the given tenant.
--   2. Check if 'richContentEnabled' key exists.
--   3. If missing, add the key and update tenant_attr in DB.
--   4. If already exists, skip update and log a notice.
-- =============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
       '<KP-TENANT-CODE>'
    ];
    v_tenant_code TEXT;
    v_old_attr JSONB;                          
    v_new_attr JSONB;                          
    v_updated BOOLEAN := false;                
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

		v_new_attr := v_old_attr;

		-- Step-2️ Add 'richContentEnabled' flag if missing
		IF (v_new_attr -> 'richContentEnabled') IS NULL THEN
			v_new_attr := jsonb_set(v_new_attr, '{richContentEnabled}', 'true'::jsonb, true);
			v_updated := true;
			RAISE NOTICE '✅ richContentEnabled flag added for tenant %', v_tenant_code;
		ELSE
			RAISE NOTICE 'richContentEnabled flag already exists for tenant %', v_tenant_code;
		END IF;

		-- Step-3️ Update the record only if modifications were made
		IF v_updated THEN
			UPDATE tenant.tenant
			SET tenant_attr = v_new_attr
			WHERE tenant_code = v_tenant_code
			  AND delete_nbr = 0;

			RAISE NOTICE '✅ tenant_attr updated successfully for tenant %', v_tenant_code;
		ELSE
			RAISE WARNING 'No changes made, all keys already exist for tenant %', v_tenant_code;
		END IF;
    END LOOP; -- tenant
END $$;
