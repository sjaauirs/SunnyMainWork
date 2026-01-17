
-- ============================================================================
-- 🚀 Script    : rollback  SweepstakesEntriesRule
-- 📌 Purpose   : rollback SweepstakesEntriesRule in tenant_attribute JSONB
-- 🧑 Author    : Kawalpreet kaur
-- 📅 Date      : 2025-11-14
-- 🧾 Jira      : RES-920
-- ⚠️ Inputs    : <TENANT-CODE>
-- 📤 Output    : rollback tenant_attribute JSONB 
-- 🔗 Script URL: <NA>
-- 📝 Notes     : This script assumes the tenant_attribute column is of type JSONB.
--               If "SweepstakesEntriesRule" already exists, it will be rollback.

-- ============================================================================
UPDATE tenant.tenant
SET tenant_attr = tenant_attr - 'sweepstakesEntriesRule'
WHERE tenant_attr ? 'sweepstakesEntriesRule' 
  AND delete_nbr = 0;


