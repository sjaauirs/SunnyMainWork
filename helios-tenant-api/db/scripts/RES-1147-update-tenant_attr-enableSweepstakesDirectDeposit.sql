-- ============================================================================
-- 🚀 Script    : Add tenant level flag in tenant_attr, If exists set the value to True
-- 📌 Purpose   : Enable Sweepstakes winners Direct Deposit based on this flag
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-Dec-04
-- 🧾 Jira      : RES-1147
-- ⚠️ Inputs    : As it is a Plat-form story give a tenant-code which you want to validate
-- 📤 Output    : enableSweepstakesDirectDeposit adds this flag all the tenants with default value false
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ============================================================================
DO $$
DECLARE
    tenant_codes TEXT[] := ARRAY['<TENANT-CODE>', '<TENANT-CODE>']; -- As it is plat-form story give a tenant-code which testing members are testing.
    rec RECORD;
    new_json jsonb;
BEGIN
    FOR rec IN
        SELECT tenant_id, tenant_code, tenant_attr
        FROM tenant.tenant
        WHERE tenant_code = ANY(tenant_codes)
    LOOP
        -- Always set flag to true (add or update)
        new_json := rec.tenant_attr || jsonb_build_object('enableSweepstakesDirectDeposit', true);

        UPDATE tenant.tenant
        SET tenant_attr = new_json
        WHERE tenant_id = rec.tenant_id;

        IF rec.tenant_attr ? 'enableSweepstakesDirectDeposit' THEN
            RAISE NOTICE 'Updated tenant % (%): Flag existed → set to true',
                rec.tenant_code, rec.tenant_id;
        ELSE
            RAISE NOTICE 'Updated tenant % (%): Flag added and set to true',
                rec.tenant_code, rec.tenant_id;
        END IF;
    END LOOP;
END $$;
