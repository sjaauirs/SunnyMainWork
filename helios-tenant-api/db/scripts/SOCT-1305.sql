-- Add the auth_config column if it doesn't exist
ALTER TABLE tenant.tenant
ADD COLUMN IF NOT EXISTS auth_config  JSONB;