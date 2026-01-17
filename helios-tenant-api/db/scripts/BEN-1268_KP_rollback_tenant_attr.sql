-- =====================================================================================================
-- 🔄 Script       : Rollback — Remove agreementDeclineImageUrl from tenant_attr JSONB
-- 📌 Purpose      : Reverts the changes applied by the forward script by removing:
--                   - agreementDeclineImageUrl
-- 🧑 Author       : Rakesh Pernati
-- 🗓️ Date         : 2025-12-02
-- 🎫 JIRA Ticket  : BEN-1268
-- ⚙️ Inputs       : Replace <KP-TENANT-CODE> with the actual tenant code
-- 📤 Output       : tenant_attr JSONB restored to its previous state (key removed)
-- 📝 Notes        :
--   - Safe to run multiple times (idempotent)
--   - Removes only the added key; no other attributes are modified
--   - If key does not exist, displays a warning and no update is performed
-- =====================================================================================================


DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        '<KP-TENANT-CODE>',
        '<KP-TENANT-CODE>'
    ];

    v_tenant_code TEXT;
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_updated BOOLEAN;
BEGIN

    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        v_updated := FALSE;

        -- Fetch tenant_attr
        SELECT tenant_attr
        INTO v_old_attr
        FROM tenant.tenant
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
          AND tenant_attr IS NOT NULL;

        IF NOT FOUND THEN
            RAISE WARNING '⚠ No tenant found or tenant_attr NULL for tenant: %', v_tenant_code;
            CONTINUE;
        END IF;

        -- If key exists, remove it
        IF (v_old_attr ? 'agreementDeclineImageUrl') THEN
            v_new_attr := v_old_attr - 'agreementDeclineImageUrl';
            v_updated := TRUE;

            RAISE NOTICE '🔄 Removing agreementDeclineImageUrl for tenant %', v_tenant_code;
        ELSE
            RAISE NOTICE '⚠ agreementDeclineImageUrl not present for tenant %, no rollback needed', v_tenant_code;
        END IF;

        -- Update only if needed
        IF v_updated THEN
            UPDATE tenant.tenant
            SET tenant_attr = v_new_attr
            WHERE tenant_code = v_tenant_code
              AND delete_nbr = 0;

            RAISE NOTICE '✅ Rollback completed for tenant %', v_tenant_code;
        END IF;

    END LOOP;

EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION '❌ Rollback failed: %', SQLERRM;
END $$;
