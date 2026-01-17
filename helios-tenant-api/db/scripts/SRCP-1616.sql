ALTER TABLE tenant.tenant 
ADD COLUMN IF NOT EXISTS enable_server_login BOOLEAN not null default false;