-- ============================================================================
-- 🚀 Script    : Updating sweepstakesEntriesRule
-- 📌 Purpose   : Adding sweepstakesEntriesRule to tenant_attributes JSONB
-- 🧑 Author    : Kawalpreet Kaur
-- 📅 Date      : 2025-11-14
-- 🧾 Jira      : RES-920
-- ⚠️ Inputs    : tenant_codes (TEXT ARRAY)
-- 📤 Output    : Updated tenant_attributes JSONB 
-- 🔗 Script URL: <NA>
-- 📝 Notes     : If sweepstakesEntriesRule exists, it will NOT overwrite
-- ============================================================================

DO $$
DECLARE 
    tenant_codes TEXT[] := ARRAY['<NAVITUS_TENANT_CODE>', '<NAVITUS_TENANT_CODE>'];  -- 🔧 <-- Input here
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr  = jsonb_set(
            tenant_attr,
            '{sweepstakesEntriesRule}',
            '{
                "entryCap": 20,
                "rolloverEnabled": true,
                "resetFrequency": "Monthly"
            }'::jsonb,
            true
        )
    WHERE tenant_attr->'sweepstakesEntriesRule' IS NULL and delete_nbr=0
      AND tenant_code = ANY(tenant_codes);
END $$;
