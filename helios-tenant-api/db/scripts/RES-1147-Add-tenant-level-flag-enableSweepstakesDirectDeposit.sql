-- ============================================================================
-- 🚀 Script    : Add tenant level flag in tenant_attr
-- 📌 Purpose   : Enable Sweepstakes winners Direct Deposit based on this flag
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 2025-Dec-04
-- 🧾 Jira      : RES-1147
-- ⚠️ Inputs    : NA
-- 📤 Output    : enableSweepstakesDirectDeposit adds this flag all the tenants with default value false
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ============================================================================

DO $$
DECLARE 
    rec RECORD;
    before_json jsonb;
    after_json jsonb;
BEGIN
    FOR rec IN
        SELECT tenant_id, tenant_code, tenant_attr
        FROM tenant.tenant
    LOOP
        before_json := rec.tenant_attr;

        -- Add the flag only if it does not exist
        IF NOT (rec.tenant_attr ? 'enableSweepstakesDirectDeposit') THEN
            UPDATE tenant.tenant
            SET tenant_attr = rec.tenant_attr || jsonb_build_object('enableSweepstakesDirectDeposit', false)
            WHERE tenant_id = rec.tenant_id;

            after_json := rec.tenant_attr || jsonb_build_object('enableSweepstakesDirectDeposit', false);

            RAISE NOTICE 'Updated tenant % (%): Added enableSweepstakesDirectDeposit=false', 
                rec.tenant_code, rec.tenant_id;
        ELSE
            RAISE NOTICE 'Skipped tenant % (%): Flag already exists', 
                rec.tenant_code, rec.tenant_id;
        END IF;
    END LOOP;
END $$;
