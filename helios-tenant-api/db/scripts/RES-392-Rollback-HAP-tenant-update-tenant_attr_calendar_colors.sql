-- 🚀 Rollback Script : Restore tenant_attr before ux.calendarColors update
-- 📌 Purpose         : Rollback changes made by RES-392 (color codes in tenant_attr->ux->calendarColors)
-- 🧑 Author          : Siva Krishna
-- 📅 Date            : 2025-09-29
-- 🧾 Jira            : RES-392
-- ⚠️ Inputs          : HAP-TENANT-CODE 
-- 📤 Output          : Removes the color codes added/updated in ux.calendarColors
-- 🔗 Script URL      : NA
-- 📝 Notes           : Run this script only if the forward script was executed.

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- 🔹 Input tenant_code
BEGIN
    -- Check if tenant exists
    IF NOT EXISTS (SELECT 1 FROM tenant.tenant WHERE tenant_code = v_tenant_code) THEN
        RAISE EXCEPTION '❌ Tenant with code "%" not found', v_tenant_code;
    END IF;

    -- If ux.calendarColors exists, remove it
    IF EXISTS (
        SELECT 1
        FROM tenant.tenant
        WHERE tenant_code = v_tenant_code
          AND tenant_attr -> 'ux' ? 'calendarColors'
    ) THEN
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux}',
            (tenant_attr -> 'ux') - 'calendarColors'
        )
        WHERE tenant_code = v_tenant_code;

        RAISE NOTICE '♻️ Rolled back ux.calendarColors for tenant: %', v_tenant_code;
    ELSE
        RAISE NOTICE 'ℹ️ No ux.calendarColors found to rollback for tenant: %', v_tenant_code;
    END IF;

    -- Show final result for verification
    RAISE NOTICE '🔎 Final tenant_attr after rollback: %',
        (SELECT tenant_attr FROM tenant.tenant WHERE tenant_code = v_tenant_code);

END $$;
