-- ============================================================================
-- 🔁 Rollback  : Remove "tableau" from tenant_attr (KP)
-- 📌 Purpose   : Removes tenant_attr.tableau key entirely
-- 🧑 Author    : Pranav Prakash
-- 📅 Date      : 2026-01-12
-- 🧾 Jira      : CPT-9
-- ⚠️ Inputs    : <KP-TENANT-CODE>
-- ============================================================================
DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];

    v_tenant_code TEXT;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE tenant.tenant
        SET tenant_attr = tenant_attr - 'tableau'
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '[INFO] Removed tableau for tenant: %', v_tenant_code;
    END LOOP;
END $$;
