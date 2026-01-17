ALTER TABLE tenant.tenant 
ADD COLUMN IF NOT EXISTS self_report BOOLEAN default false;