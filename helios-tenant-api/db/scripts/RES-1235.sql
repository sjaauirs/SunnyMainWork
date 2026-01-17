-- ============================================================================
-- 🚀 Script    : Updating showSelectedPrimaryMenuItem for navitus
-- 📌 Purpose   : Adding showSelectedPrimaryMenuItem  to tenant_option_json JSONB
-- 🧑 Author    : Kawalpreet Kaur
-- 📅 Date      : 2025-11-18
-- 🧾 Jira      : RES-1235
-- ⚠️ Inputs    : tenant_codes (TEXT ARRAY)
-- 📤 Output    : Updated tenant_option_json JSONB 
-- 🔗 Script URL: <NA>
-- 📝 Notes     : If showSelectedPrimaryMenuItem  exists, it will NOT overwrite
-- ============================================================================
DO $$
DECLARE 
    tenant_codes TEXT[] := ARRAY['<NAVITUS_TENANT_CODE>', 'NAVITUS_TENANT_CODE'];  -- 🔧 <-- Input here
BEGIN
    UPDATE tenant.tenant
    SET tenant_option_json  = jsonb_set(
            tenant_option_json,
            '{showSelectedPrimaryMenuItem}',
            'true'::jsonb,
            true
        )
    WHERE  delete_nbr=0
      AND tenant_code = ANY(tenant_codes);
END $$;