-- =====================================================================================================
-- 🔁 Script       : Rollback - Remove isPhoneNumberEditable & memberPortalHyperlinkUrl from tenant_attr
-- 📌 Purpose      : Safely removes specific keys from tenant_attr JSONB for a given tenant
-- 🧑 Author       : Rakesh Pernati
-- 🗓️ Date         : 2025-10-23
-- 🎫 JIRA Ticket  : BEN-855
-- 🧾 Description  :
--   - Removes the following keys (if they exist):
--       • isPhoneNumberEditable
--       • memberPortalHyperlinkUrl
--   - Ensures tenant_attr is not NULL or empty before attempting modification
--   - Logs all actions (removed / not found / no tenant found)
-- ⚙️ Inputs       : Replace <NAVITUS-TENANT-CODE> with the actual tenant code
-- 📤 Output       : tenant_attr JSONB reverted to previous state (without these keys)
-- 📝 Notes        :
--   - Safe to execute multiple times (idempotent rollback)
--   - Will skip update if tenant_attr or tenant record not found
-- =====================================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<NAVITUS-TENANT-CODE>'; -- Input tenant code
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_updated BOOLEAN := false;
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

    -- Remove isPhoneNumberEditable if it exists
    IF (v_new_attr ? 'isPhoneNumberEditable') THEN
        v_new_attr := v_new_attr - 'isPhoneNumberEditable';
        v_updated := true;
        RAISE NOTICE 'isPhoneNumberEditable key removed for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'isPhoneNumberEditable key not found for tenant %', v_tenant_code;
    END IF;

    -- Remove memberPortalHyperlinkUrl if it exists
    IF (v_new_attr ? 'memberPortalHyperlinkUrl') THEN
        v_new_attr := v_new_attr - 'memberPortalHyperlinkUrl';
        v_updated := true;
        RAISE NOTICE 'memberPortalHyperlinkUrl key removed for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'memberPortalHyperlinkUrl key not found for tenant %', v_tenant_code;
    END IF;

    -- Update tenant_attr only if something was removed
    IF v_updated THEN
        UPDATE tenant.tenant
        SET tenant_attr = v_new_attr
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE 'Rollback successful — tenant_attr updated for tenant %', v_tenant_code;
    ELSE
        RAISE WARNING 'No keys found to remove for tenant %', v_tenant_code;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Failed to rollback tenant_attr : %', SQLERRM;
END $$;
