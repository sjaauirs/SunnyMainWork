ALTER TABLE tenant.tenant 
ADD COLUMN IF NOT EXISTS tenant_option_json jsonb not null default '{"apps": ["REWARDS"]}';