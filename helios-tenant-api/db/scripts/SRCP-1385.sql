ALTER TABLE tenant.tenant 
ADD COLUMN IF NOT EXISTS direct_login BOOLEAN not null default false;