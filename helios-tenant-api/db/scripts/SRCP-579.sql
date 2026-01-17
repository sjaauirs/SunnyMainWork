ALTER TABLE tenant.tenant
ADD COLUMN tenant_attr jsonb default '{}'

ALTER TABLE tenant.tenant
ALTER COLUMN tenant_attr SET NOT NULL