-- ============================================================================
-- 🚀 Script    : Rollback rewardsSplashButton* colors from tenant_attr JSONB
-- 📌 Purpose   : Removes the following keys from ux.agreementColors:
--                   - rewardsSplashButtonColor
--                   - rewardsSplashButtonLabelColor
--                   - rewardsSplashButtonBorderColor
-- 🧑 Author    : Rakesh Pernati
-- 📅 Date      : 2025-09-23
-- 🧾 Jira      : BEN-571 (Rollback)
-- ⚠️ Inputs    : v_tenant_code (Tenant identifier)
-- 📤 Output    : Cleans up tenant_attr JSONB by removing inserted keys
-- 🔗 Script URL: <Optional documentation or Confluence link>
-- 📝 Notes     : Safe rollback; ignores keys if not present
-- ============================================================================


DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- Input tenant code
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

    -- Remove rewardsSplashButtonColor
    IF (v_new_attr #>> '{ux,agreementColors,rewardsSplashButtonColor}') IS NOT NULL THEN
        v_new_attr := v_new_attr #- '{ux,agreementColors,rewardsSplashButtonColor}';
        v_updated := true;
        RAISE NOTICE 'Removed rewardsSplashButtonColor for tenant %', v_tenant_code;
    END IF;

    -- Remove rewardsSplashButtonLabelColor
    IF (v_new_attr #>> '{ux,agreementColors,rewardsSplashButtonLabelColor}') IS NOT NULL THEN
        v_new_attr := v_new_attr #- '{ux,agreementColors,rewardsSplashButtonLabelColor}';
        v_updated := true;
        RAISE NOTICE 'Removed rewardsSplashButtonLabelColor for tenant %', v_tenant_code;
    END IF;

    -- Remove rewardsSplashButtonBorderColor
    IF (v_new_attr #>> '{ux,agreementColors,rewardsSplashButtonBorderColor}') IS NOT NULL THEN
        v_new_attr := v_new_attr #- '{ux,agreementColors,rewardsSplashButtonBorderColor}';
        v_updated := true;
        RAISE NOTICE 'Removed rewardsSplashButtonBorderColor for tenant %', v_tenant_code;
    END IF;

    -- Update tenant_attr only if changes were made
    IF v_updated THEN
        UPDATE tenant.tenant
           SET tenant_attr = v_new_attr
         WHERE tenant_code = v_tenant_code
           AND delete_nbr = 0;
        RAISE NOTICE 'Rollback completed: tenant_attr updated for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'No rollback needed, keys not found for tenant %', v_tenant_code;
    END IF;
END $$;
