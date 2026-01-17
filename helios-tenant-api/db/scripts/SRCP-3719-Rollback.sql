-- This is a rollback script for SRCP-3719.sql
UPDATE tenant.tenant
SET tenant_attr = tenant_attr - 'membershipWallet'
WHERE tenant_attr IS NOT NULL
  AND tenant_attr <> '{}'::jsonb
  AND delete_nbr = 0;
