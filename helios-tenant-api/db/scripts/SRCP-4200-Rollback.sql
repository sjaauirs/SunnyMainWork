-- This is a rollback script for SRCP-4200-update_tenant_attr_start_page.sql
UPDATE tenant.tenant
SET tenant_attr = tenant_attr - 'startPage'  -- Removes the startPage key from tenant_attr
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb 
  AND delete_nbr = 0;
