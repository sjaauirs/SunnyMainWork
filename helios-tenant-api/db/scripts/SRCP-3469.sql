-- Update Tenant attribute:
-- justInTimeFunding for this initial value keeping as true
-- jitfTimeOffset for this initial value keeping as 40 default value

-- Update each tenant's tenant_attr with default values
UPDATE tenant.tenant 
SET tenant_attr = jsonb_set(
    jsonb_set(
        tenant_attr, 
        '{justInTimeFunding}', 
        'false', true
    ),
    '{jitfTimeOffset}',
    '40',true
)
WHERE delete_nbr = 0;
