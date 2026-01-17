ALTER TABLE tenant.tenant 
ADD COLUMN IF NOT EXISTS tenant_name varchar(80) not null default 'test';