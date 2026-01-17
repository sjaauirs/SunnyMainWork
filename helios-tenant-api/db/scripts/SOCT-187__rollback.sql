-- This is Rollback script for SRCP-187__add_spendOnGiftCardDisabled_flag.sql
-- Rollback script to remove the "spendOnGiftCardDisabled" flag from the tenant attribute JSON.
-- This will delete the key if it exists.

UPDATE tenant.tenant
SET tenant_attr = tenant_attr - 'spendOnGiftCardDisabled'  -- Remove the key from the JSONB column
WHERE tenant_attr IS NOT NULL 
  AND tenant_attr <> '{}'::jsonb  
  AND delete_nbr = 0;
