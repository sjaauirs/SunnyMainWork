-- ============================================================================
-- 🚀 Script    : Updating enableSweepstakesInfoIcon for navitus
-- 📌 Purpose   : Adding enableSweepstakesInfoIcon  to tenant_attributes JSONB
-- 🧑 Author    : Kawalpreet Kaur
-- 📅 Date      : 2025-12-05
-- 🧾 Jira      : RES-1254
-- ⚠️ Inputs    : tenant_codes (TEXT ARRAY)
-- 📤 Output    : Updated tenant_attributes JSONB 
-- 🔗 Script URL: <NA>
-- 📝 Notes     : If enableSweepstakesInfoIcon  exists, it will NOT overwrite
-- ============================================================================

DO $$
DECLARE 
    tenant_codes TEXT[] := ARRAY['<Watco_TENANT_CODE>', '<Watco_TENANT_CODE>'];  -- 🔧 <-- Input here
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr  = jsonb_set(
            tenant_attr,
            '{enableSweepstakesInfoIcon }',
            'true'::jsonb,
            true
        )
    WHERE  delete_nbr=0
      AND tenant_code = ANY(tenant_codes);
END $$;

