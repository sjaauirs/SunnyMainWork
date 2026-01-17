-- =====================================================================================================
-- 🚀 Script       : Add/Update isPhoneNumberEditable & memberPortalHyperlinkUrl in tenant_attr JSONB
-- 📌 Purpose      : Ensures that tenant_attr JSONB for the given tenant contains the following keys:
--                   - isPhoneNumberEditable
--                   - memberPortalHyperlinkUrl
-- 🧑 Author       : Rakesh Pernati
-- 🗓️ Date         : 2025-10-24
-- 🧾 Description  :
--   - Ensures tenant_attr is not NULL or empty ({})
--   - Adds missing JSONB keys if they don’t exist
--   - Logs actions taken for visibility (added/existing/no change)
--   - Updates tenant_attr only if modifications were made
-- 🎫 JIRA Ticket  : BEN-855
-- ⚙️ Inputs       : Replace <NAVITUS-TENANT-CODE> with the actual tenant code
-- 📤 Output       : Updated tenant_attr JSONB with required flags
-- 📝 Notes        :
--   - Safe to run multiple times (idempotent)
--   - If no tenant found or tenant_attr is empty, displays a warning
-- =====================================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<NAVITUS-TENANT-CODE>'; -- Input tenant code
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_updated BOOLEAN := false;
    v_member_portal_url TEXT := '';
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
        RAISE WARNING '⚠️ No tenant found or tenant_attr is empty for tenant_code: %', v_tenant_code;
        RETURN;
    END IF;

    v_new_attr := v_old_attr;

    -- Add isPhoneNumberEditable flag if missing
    IF (v_new_attr -> 'isPhoneNumberEditable') IS NULL THEN
        v_new_attr := jsonb_set(v_new_attr, '{isPhoneNumberEditable}', 'true'::jsonb, true);
        v_updated := true;
        RAISE NOTICE '✅isPhoneNumberEditable flag added for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE '⚠️isPhoneNumberEditable flag already exists for tenant %', v_tenant_code;
    END IF;

    -- Add memberPortalHyperlinkUrl if missing
    IF (v_new_attr -> 'memberPortalHyperlinkUrl') IS NULL THEN
        v_new_attr := jsonb_set(v_new_attr, '{memberPortalHyperlinkUrl}', to_jsonb(v_member_portal_url), true);
        v_updated := true;
        RAISE NOTICE '✅memberPortalHyperlinkUrl added for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE '⚠️memberPortalHyperlinkUrl already exists for tenant %', v_tenant_code;
    END IF;

    -- Update only if changes were made
    IF v_updated THEN
        UPDATE tenant.tenant
        SET tenant_attr = v_new_attr
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '✅tenant_attr updated successfully for tenant %', v_tenant_code;
    ELSE
        RAISE WARNING '⚠️No changes made — all keys already exist for tenant %', v_tenant_code;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION '❌Failed to update tenant_attr : %', SQLERRM;
END $$;
