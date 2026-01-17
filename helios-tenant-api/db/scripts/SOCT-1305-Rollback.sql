-- Remove the auth_config column if it exists
ALTER TABLE tenant.tenant
DROP COLUMN IF EXISTS auth_config;
