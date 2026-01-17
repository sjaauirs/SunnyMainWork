-- =============================================================================
-- Purpose : Add/Update onboardingSurvey and pickAPurseOnboardingEnabled in tenant_attr
-- Notes   : Idempotent; updates only when needed
-- Jira	   : BEN-60
-- =============================================================================
DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- Input tenant code
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_updated BOOLEAN := false;
BEGIN
    -- Fetch current tenant_attr (must exist and be non-empty)
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


    -- Ensure pickAPurseOnboardingEnabled = true (set if missing or not true)
    IF (v_new_attr->>'pickAPurseOnboardingEnabled') IS DISTINCT FROM 'true' THEN
        v_new_attr := jsonb_set(v_new_attr, '{pickAPurseOnboardingEnabled}', 'true'::jsonb, true);
        v_updated := true;
        RAISE NOTICE 'pickAPurseOnboardingEnabled set to true for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE 'pickAPurseOnboardingEnabled already true for tenant %', v_tenant_code;
    END IF;

    -- Apply update only if something changed
    IF v_updated THEN
        UPDATE tenant.tenant
           SET tenant_attr = v_new_attr
         WHERE tenant_code = v_tenant_code
           AND delete_nbr = 0;

        RAISE NOTICE 'tenant_attr updated successfully for tenant %', v_tenant_code;
    ELSE
        RAISE WARNING 'No changes made; all keys already set for tenant %', v_tenant_code;
    END IF;
END $$;
