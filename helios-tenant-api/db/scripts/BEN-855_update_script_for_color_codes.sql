-- ============================================================================
-- 🚀 Script    : Insert/Update tenant_attr Colors (phoneNumberColor)
-- 📌 Purpose   : Inserts or updates color values (phoneNumberColor)
--               in tenant_attr JSONB for the specified tenant.
-- 🧑 Author    : Rakesh Pernati
-- 📅 Date      : 2025-10-23
-- 🧾 Jira      : BEN-855
-- ⚠️ Inputs    : Replace v_tenant_code with the actual tenant code before execution.
-- 📤 Output    : Ensures the tenant_attr JSONB has the correct color configurations.
-- 🔗 Script URL: <Optional documentation or Confluence link>
-- 📝 Notes     : 
--               - Idempotent script: safe to run multiple times.
--               - Updates only if color values differ or are missing.
--               - Logs insert, update, or no-change messages for each color key.
-- ============================================================================


DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- Input tenant code
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_updated BOOLEAN := false;

    -- Constants
    v_phone_number_color TEXT := '#5C5F66';
   
BEGIN
    -- Fetch current tenant_attr
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

    -- Insert/Update phoneNumberColor
    IF (v_new_attr #>> '{ux,commonColors,phoneNumberColor}') IS DISTINCT FROM v_phone_number_color THEN
        v_new_attr := jsonb_set(v_new_attr, '{ux,commonColors,phoneNumberColor}', to_jsonb(v_phone_number_color), true);
        v_updated := true;

        IF (v_old_attr #>> '{ux,commonColors,phoneNumberColor}') IS NULL THEN
            RAISE NOTICE 'phoneNumberColor inserted with value % for tenant %', v_phone_number_color, v_tenant_code;
        ELSE
            RAISE NOTICE 'phoneNumberColor updated to value % for tenant %', v_phone_number_color, v_tenant_code;
        END IF;
    ELSE
        RAISE NOTICE 'phoneNumberColor already set to % for tenant % (no change)', v_phone_number_color, v_tenant_code;
    END IF;

    -- Apply update
    IF v_updated THEN
        UPDATE tenant.tenant
        SET tenant_attr = v_new_attr
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '✅ tenant_attr updated successfully for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'ℹ️ No changes made, tenant_attr already has correct values for tenant %', v_tenant_code;
    END IF;
END $$;
