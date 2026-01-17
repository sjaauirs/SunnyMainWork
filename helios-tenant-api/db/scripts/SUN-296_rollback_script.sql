-- Author      : Pernati Rakesh
-- Purpose     : Revert 'ux.triviaColors' addition by removing it from tenant_attr JSONB
-- Jira Task   : SUN-296

DO $$
DECLARE
    rec RECORD;
BEGIN
    FOR rec IN
        SELECT tenant_id, tenant_attr
        FROM tenant.tenant
        WHERE delete_nbr = 0
          AND tenant_attr IS NOT NULL
          AND tenant_attr <> '{}'::jsonb
          AND (tenant_attr->'ux') ? 'triviaColors'
    LOOP
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
                              tenant_attr,
                              '{ux}',
                              (tenant_attr->'ux') - 'triviaColors',  -- Remove 'triviaColors' key from 'ux'
                              false
                          ),
            update_ts = NOW(),
            update_user = 'SYSTEM'
        WHERE tenant_id = rec.tenant_id;

        RAISE NOTICE '♻️ Reverted triviaColors for tenant_id: %', rec.tenant_id;
    END LOOP;
END $$;
