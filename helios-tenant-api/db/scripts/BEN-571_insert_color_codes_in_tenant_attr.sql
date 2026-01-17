-- ============================================================================
-- 🚀 Script    : Add/Update rewardsSplashButton* colors in tenant_attr JSONB
-- 📌 Purpose   : Ensures tenant_attr contains the following keys under ux.agreementColors:
--                   - rewardsSplashButtonColor (#181D27)
--                   - rewardsSplashButtonLabelColor (#FFFFFF)
--                   - rewardsSplashButtonBorderColor (#181D27)
-- 🧑 Author    : Rakesh Pernati
-- 📅 Date      : 2025-09-23
-- 🧾 Jira      : BEN-571
-- ⚠️ Inputs    : v_tenant_code (Tenant identifier)
-- 📤 Output    : Updates tenant_attr JSONB with new keys if missing
-- 🔗 Script URL: <Optional documentation or Confluence link>
-- 📝 Notes     : Runs idempotently; does not overwrite existing values
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

    -- Add rewardsSplashButtonColor if missing
    IF (v_new_attr #>> '{ux,agreementColors,rewardsSplashButtonColor}') IS NULL THEN
        v_new_attr := jsonb_set(v_new_attr, '{ux,agreementColors,rewardsSplashButtonColor}', to_jsonb('#FFFFFF'::text), true);
        v_updated := true;
        RAISE NOTICE 'Added rewardsSplashButtonColor for tenant %', v_tenant_code;
    END IF;

    -- Add rewardsSplashButtonLabelColor if missing
    IF (v_new_attr #>> '{ux,agreementColors,rewardsSplashButtonLabelColor}') IS NULL THEN
        v_new_attr := jsonb_set(v_new_attr, '{ux,agreementColors,rewardsSplashButtonLabelColor}', to_jsonb('##181D27'::text), true);
        v_updated := true;
        RAISE NOTICE 'Added rewardsSplashButtonLabelColor for tenant %', v_tenant_code;
    END IF;

    -- Add rewardsSplashButtonBorderColor if missing
    IF (v_new_attr #>> '{ux,agreementColors,rewardsSplashButtonBorderColor}') IS NULL THEN
        v_new_attr := jsonb_set(v_new_attr, '{ux,agreementColors,rewardsSplashButtonBorderColor}', to_jsonb('#181D27'::text), true);
        v_updated := true;
        RAISE NOTICE 'Added rewardsSplashButtonBorderColor for tenant %', v_tenant_code;
    END IF;

    -- Update tenant_attr only if changes were made
    IF v_updated THEN
        UPDATE tenant.tenant
           SET tenant_attr = v_new_attr
         WHERE tenant_code = v_tenant_code
           AND delete_nbr = 0;
        RAISE NOTICE 'tenant_attr updated successfully for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'No changes made, all keys already exist for tenant %', v_tenant_code;
    END IF;
END $$;
