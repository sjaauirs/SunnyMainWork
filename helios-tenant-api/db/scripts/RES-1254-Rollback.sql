-- ============================================================================
-- 🚀 Script    : rollback enableSweepstakesInfoIcon for tenant
-- 📌 Purpose   : rollback enableSweepstakesInfoIcon  to tenant_attributes JSONB
-- 🧑 Author    : Kawalpreet Kaur
-- 📅 Date      : 2025-12-05
-- 🧾 Jira      : RES-1254
-- ⚠️ Inputs    : 
-- 📤 Output    : Updated tenant_attributes JSONB 
-- 🔗 Script URL: <NA>
-- 📝 Notes     : If enableSweepstakesInfoIcon  exists, it will NOT overwrite
-- ============================================================================

DO $$
DECLARE 
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr  = 
            tenant_attr -
            'enableSweepstakesInfoIcon ',
            
    WHERE  delete_nbr=0
END $$;

