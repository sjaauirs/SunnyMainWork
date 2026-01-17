-- ============================================================================
-- 🚀 Script    : Enforce all agreementColors in tenant_attr JSONB
-- 📌 Purpose   : Ensures tenant_attr.ux.agreementColors has the correct palette
-- 🧑 Author    : Saurabh
-- 📅 Date      : 2025-09-29
-- 🧾 Jira      : BEN-180
-- ⚠️ Inputs    : v_tenant_code (Tenant identifier)
-- 📤 Output    : Updates or creates agreementColors JSONB with enforced values
-- 📝 Notes     : Idempotent, always resets to the expected palette
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- Input tenant code
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

    -- Overwrite / Create agreementColors
    v_new_attr := jsonb_set(
        v_new_attr,
        '{ux,agreementColors}',
        '{
           	"agreeButtonColor": "#181D27",
            "agreeCheckboxColor": "#0078B3",
            "agreeButtonLabelColor": "#FFFFFF",
            "declineButtonLabelColor": "#181D27",
            "declineButton1LabelColor": "#181D27",
            "rewardsSplashButtonColor": "#0078B3",
            "rewardsSplashButtonLabelColor": "#FFFFFF",
            "rewardsSplashDisabledLabelColor": "#858D9C",
            "rewardsSplashDisabledButtonColor": "#D3D6DC"
        }'::jsonb,
        true
    );
    v_updated := true;

    -- Apply update
    IF v_updated THEN
        UPDATE tenant.tenant
           SET tenant_attr = v_new_attr
         WHERE tenant_code = v_tenant_code
           AND delete_nbr = 0;

        RAISE NOTICE 'tenant_attr.ux.agreementColors enforced successfully for tenant %', v_tenant_code;
    END IF;
END $$;
