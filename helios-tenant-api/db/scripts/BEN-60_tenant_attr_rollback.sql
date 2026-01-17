

-- ================================================================
-- Soft Rollback : Disable flag(s) without deleting keys
-- Effect        : pickAPurseOnboardingEnabled=false 
-- JIRA			 : BEN-60
-- ================================================================
DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>';
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_updated  BOOLEAN := false;
BEGIN
    SELECT tenant_attr
      INTO v_old_attr
      FROM tenant.tenant
     WHERE tenant_code = v_tenant_code
       AND delete_nbr = 0
       AND tenant_attr IS NOT NULL
       AND tenant_attr::text <> '{}';

    IF NOT FOUND THEN
        RAISE WARNING 'No tenant found or empty tenant_attr for %', v_tenant_code;
        RETURN;
    END IF;

    v_new_attr := v_old_attr;

    -- Only flip pickAPurseOnboardingEnabled to false (idempotent)
    IF (v_new_attr->>'pickAPurseOnboardingEnabled') IS DISTINCT FROM 'false' THEN
        v_new_attr := jsonb_set(v_new_attr, '{pickAPurseOnboardingEnabled}', 'false'::jsonb, true);
        v_updated := true;
        RAISE NOTICE 'pickAPurseOnboardingEnabled set to false for %', v_tenant_code;
    END IF;

    IF v_updated THEN
        UPDATE tenant.tenant
           SET tenant_attr = v_new_attr
         WHERE tenant_code = v_tenant_code
           AND delete_nbr = 0;
        RAISE NOTICE 'tenant_attr updated (soft rollback) for %', v_tenant_code;
    ELSE
        RAISE NOTICE 'No changes needed (already soft-rolled back) for %', v_tenant_code;
    END IF;
END $$;
